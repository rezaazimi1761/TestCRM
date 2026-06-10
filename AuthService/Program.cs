using System.Text;
using AuthService.Application.Consumers;
using AuthService.Infrastructure.Persistence;
using AuthService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Database ───────────────────────────────────────────────────
builder.Services.AddDbContext<AuthDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── JWT Auth ───────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
var key        = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Secret"]!));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSection["Issuer"],
            ValidAudience            = jwtSection["Audience"],
            IssuerSigningKey         = key,
            ClockSkew                = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ── App Services ───────────────────────────────────────────────
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IClaimManagerService, ClaimManagerService>();

// ── MassTransit + RabbitMQ + EF Core Inbox ──────────────────────
// Inbox guarantees: each UserCreatedEvent is processed exactly once.
// MassTransit records the MessageId in InboxState. If the message is
// delivered twice (at-least-once delivery), the second attempt is a no-op.
// The consumer + DB write happen inside one transaction — if the consumer
// throws, the InboxState row is NOT committed and the message is retried.
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserCreatedConsumer>();

    // EF Core Inbox — wraps each consumer invocation in a transaction
    // that commits InboxState + the consumer's own DB changes atomically.
    x.AddEntityFrameworkOutbox<AuthDbContext>(o =>
    {
        o.UseSqlServer();
        o.QueryDelay = TimeSpan.FromSeconds(2);
    });

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var rb = builder.Configuration.GetSection("RabbitMQ");
        cfg.Host(rb["Host"] ?? "rabbitmq://localhost", h =>
        {
            h.Username(rb["Username"] ?? "guest");
            h.Password(rb["Password"] ?? "guest");
        });

        cfg.ReceiveEndpoint("authservice-user-created", e =>
        {
            // Retry: 3 attempts with increasing delays.
            // After the 3rd failure MassTransit automatically publishes
            // Fault<UserCreatedEvent> to RabbitMQ — TestCRM consumes it
            // and soft-deletes the pending CRM user (compensation).
            e.UseMessageRetry(r => r.Intervals(
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(15),
                TimeSpan.FromSeconds(30)));

            // Inbox: InboxState + AppUser insert in ONE transaction.
            // Consumer throw → rollback → retry (InboxState not committed).
            e.UseEntityFrameworkOutbox<AuthDbContext>(ctx);

            e.ConfigureConsumer<UserCreatedConsumer>(ctx);
        });
    });
});

// ── gRPC ───────────────────────────────────────────────────────
builder.Services.AddGrpc();

// ── REST ───────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthService API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In     = ParameterLocation.Header,
        Name   = "Authorization",
        Type   = SecuritySchemeType.Http,
        Scheme = "bearer"
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

var app = builder.Build();

// ── Auto-migrate + seed default tenant & master admin on startup ─
using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    db.Database.Migrate();
    await DataSeeder.SeedAsync(db, app.Configuration, logger);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<AuthGrpcService>();

app.Run();
