using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModernCRM.Auth.Infrastructure.Integration;
using ModernCRM.Auth.Infrastructure.Persistence;
using ModernCRM.Auth.Application.Integration;

namespace ModernCRM.Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be provided through a secure configuration source.");
        services.AddDbContext<AuthDbContext>(options => options.UseSqlServer(connectionString));
        services.AddDbContext<AuthIntegrationDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IAuthPersistenceRepository, AuthPersistenceRepository>();
        services.AddScoped<AuthDatabaseInitializer>();
        return services;
    }

    public static void AddAuthEfOutbox(this IBusRegistrationConfigurator configurator)
        => configurator.AddEntityFrameworkOutbox<AuthIntegrationDbContext>(options =>
        {
            options.UseSqlServer();
            options.QueryDelay = TimeSpan.FromSeconds(1);
            options.UseBusOutbox();
        });

    public static void UseAuthEfOutbox(this IReceiveEndpointConfigurator endpoint, IRegistrationContext context)
        => endpoint.UseEntityFrameworkOutbox<AuthIntegrationDbContext>(context);
}
