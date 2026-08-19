using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModernCRM.Crm.Infrastructure.Integration;
using ModernCRM.Crm.Infrastructure.Persistence;
using ModernCRM.Crm.Application.Users;
using ModernCRM.Crm.Application.Frontend;

namespace ModernCRM.Crm.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCrmPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be provided through a secure configuration source.");
        services.AddDbContext<CrmDbContext>(options => options.UseSqlServer(connectionString));
        services.AddDbContext<CrmIntegrationDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<ICrmFrontendRepository, CrmPersistenceRepository>();
        services.AddScoped<ICrmUserService, CrmUserService>();
        services.AddScoped<FrontendCrudService>();
        services.AddScoped<ILeadService>(provider => provider.GetRequiredService<FrontendCrudService>());
        services.AddScoped<IActivityService>(provider => provider.GetRequiredService<FrontendCrudService>());
        services.AddScoped<CrmDatabaseInitializer>();
        return services;
    }

    public static void AddCrmEfOutbox(this IBusRegistrationConfigurator configurator)
        => configurator.AddEntityFrameworkOutbox<CrmIntegrationDbContext>(options =>
        {
            options.UseSqlServer();
            options.QueryDelay = TimeSpan.FromSeconds(1);
            options.UseBusOutbox();
        });

    public static void UseCrmSagaRepository(this IEntityFrameworkSagaRepositoryConfigurator<UserSyncSagaState> repository)
    {
        repository.ConcurrencyMode = ConcurrencyMode.Pessimistic;
        repository.ExistingDbContext<CrmIntegrationDbContext>();
        repository.UseSqlServer();
    }

    public static void UseCrmEfOutbox(this IReceiveEndpointConfigurator endpoint, IRegistrationContext context)
        => endpoint.UseEntityFrameworkOutbox<CrmIntegrationDbContext>(context);
}
