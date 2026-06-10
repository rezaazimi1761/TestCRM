using System.Threading.RateLimiting;
using MassTransit;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Shared.Application.Interfaces;
using TestCRM.Infrastructure.GrpcClients;
using TestCRM.Infrastructure.Middleware;
using TestCRM.Infrastructure.Persistence;
using Shared.Infrastructure.Services;
using TestCRM.Infrastructure.ServiceInstance;

var builder = WebApplication.CreateBuilder(args);

// ── Kestrel: suppress "Server: Kestrel" response header ────────
builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false);

// ── HttpContext / Tenant ────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();

// ── Database ────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── MediatR ─────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

// ── MassTransit + RabbitMQ + EF Core Outbox ─────────────────────
// Outbox guarantees: UserCreatedEvent is written to OutboxMessage table
// in the SAME transaction as the AppUser insert. The OutboxDeliveryService
// background worker reads pending rows and forwards them to RabbitMQ.
// If the app crashes before delivery, the message survives in SQL Server
// and is retried on next startup — no user creation is lost.
builder.Services.AddMassTransit(x =>
{
    // EF Core Outbox — publisher side only (no consumers in TestCRM)
    x.AddEntityFrameworkOutbox<AppDbContext>(o =>
    {
        o.UseSqlServer();
        o.QueryDelay = TimeSpan.FromSeconds(2);
        // UseBusOutbox routes ALL IBus/IPublishEndpoint calls through the
        // OutboxMessage table instead of going directly to RabbitMQ.
        // Without this, IPublishEndpoint tries to connect to RabbitMQ
        // immediately and blocks/fails when the broker is unavailable.
        o.UseBusOutbox();
    });

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var rb = builder.Configuration.GetSection("RabbitMQ");
        cfg.Host(rb["Host"] ?? "rabbitmq://localhost", h =>
        {
            h.Username(rb["Username"] ?? "guest");
            h.Password(rb["Password"] ?? "guest");
        });
        cfg.ConfigureEndpoints(ctx);
    });
});

// ── gRPC client to AuthService ──────────────────────────────────
builder.Services.AddSingleton<IAuthGrpcClient, AuthGrpcClient>();

// ── Rate Limiting (fixed window: 200 req / 10 s per IP) ─────────
builder.Services.AddRateLimiter(opts =>
{
    opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    opts.AddFixedWindowLimiter("api", o =>
    {
        o.Window            = TimeSpan.FromSeconds(10);
        o.PermitLimit       = 200;
        o.QueueLimit        = 0;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

// ── REST + Swagger ──────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(options =>
    {
        // Return a simple { "message": "..." } instead of the verbose
        // ValidationProblemDetails JSON so the Blazor frontend can display it cleanly.
        options.InvalidModelStateResponseFactory = ctx =>
        {
            var firstError = ctx.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Validation failed.";

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
                new { message = firstError });
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CRM API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In          = ParameterLocation.Header,
        Name        = "Authorization",
        Type        = SecuritySchemeType.Http,
        Scheme      = "bearer",
        Description = "Paste the JWT from AuthService here"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                    { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthorization();

// ── Self-register this CRM instance with AuthService on first boot ─
builder.Services.AddHttpClient();
builder.Services.AddHostedService<ServiceInstanceBootstrapper>();

var app = builder.Build();

// ── Auto-migrate on startup ─────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ── Rate limiting ───────────────────────────────────────────────
app.UseRateLimiter();

// ── Custom JWT middleware (validates via AuthService gRPC) ───────
app.UseMiddleware<JwtAuthMiddleware>();

app.UseAuthorization();
app.MapControllers().RequireRateLimiting("api");

app.Run();
