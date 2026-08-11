using ModernCRM.Auth.Api.UserSync;
using ModernCRM.Auth.Api.Consumers;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using ModernCRM.Auth.Api.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ModernCRM.Auth.Application.Handlers;
using ModernCRM.Auth.Domain.Users;
using ModernCRM.Auth.Infrastructure.Identity;
using ModernCRM.Auth.Infrastructure.Persistence;
using ModernCRM.Auth.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
if (jwtSecret.Length < 32) throw new InvalidOperationException("Jwt:Secret must contain at least 32 characters.");

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAuthUserRepository, AuthUserRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

builder.Services.AddScoped<CreateUserHandler>();
builder.Services.AddScoped<UpdateUserHandler>();
builder.Services.AddScoped<DeleteUserHandler>();
builder.Services.AddScoped<GetUsersHandler>();
builder.Services.AddScoped<GetUserByIdHandler>();
builder.Services.AddScoped<CreateTenantHandler>();
builder.Services.AddScoped<UpdateTenantHandler>();
builder.Services.AddScoped<GetTenantsHandler>();
builder.Services.AddScoped<GetTenantByIdHandler>();
builder.Services.AddDbContext<AuthIntegrationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SyncUserToAuthConsumer>();
    x.AddEntityFrameworkOutbox<AuthIntegrationDbContext>(o =>
    {
        o.UseSqlServer();
        o.QueryDelay = TimeSpan.FromSeconds(1);
        o.UseBusOutbox();
    });
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbit = builder.Configuration.GetSection("RabbitMQ");
        cfg.Host(rabbit["Host"] ?? "rabbitmq://localhost", h =>
        {
            h.Username(rabbit["Username"] ?? "guest");
            h.Password(rabbit["Password"] ?? "guest");
        });
        cfg.ReceiveEndpoint("moderncrm-sync-user-to-auth", e =>
        {
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
            e.UseEntityFrameworkOutbox<AuthIntegrationDbContext>(context);
            e.ConfigureConsumer<SyncUserToAuthConsumer>(context);
        });
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ModernCRM.Auth.Api.ExceptionHandling.DomainExceptionHandler>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<AuthDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<AuthIntegrationDbContext>().Database.MigrateAsync();
    var integrationDb = scope.ServiceProvider.GetRequiredService<AuthIntegrationDbContext>();
    if (!await integrationDb.ServiceInstances.AnyAsync())
    {
        integrationDb.ServiceInstances.Add(new ServiceInstanceModel
        {
            Id = app.Configuration.GetValue<Guid>("Seed:DefaultServiceInstanceId"),
            Name = app.Configuration["Seed:DefaultServiceInstanceName"] ?? "crm-local",
            ApiUrl = app.Configuration["Seed:DefaultServiceInstanceUrl"] ?? "http://localhost:9040",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await integrationDb.SaveChangesAsync();
    }
    await AuthDataSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AuthDbContext>(), scope.ServiceProvider.GetRequiredService<IPasswordHasher>(), app.Configuration);
}

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
