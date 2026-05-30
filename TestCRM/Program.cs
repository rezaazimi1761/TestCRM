using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Shared.Application.Interfaces;
using TestCRM.Infrastructure.GrpcClients;
using TestCRM.Infrastructure.Middleware;
using TestCRM.Infrastructure.Persistence;
using Shared.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ── HttpContext / Tenant ────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();

// ── Database ────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── MediatR ─────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

// ── gRPC client to AuthService ──────────────────────────────────
builder.Services.AddSingleton<IAuthGrpcClient, AuthGrpcClient>();

// ── REST + Swagger ──────────────────────────────────────────────
builder.Services.AddControllers();
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

// ── Custom JWT middleware (validates via AuthService gRPC) ───────
app.UseMiddleware<JwtAuthMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();
