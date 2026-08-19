using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ModernCRM.Auth.Infrastructure.Integration;
using ModernCRM.Auth.Application.Handlers;
using ModernCRM.Auth.Infrastructure.Persistence;
using ModernCRM.Auth.Application.Integration;

namespace ModernCRM.Auth.Infrastructure.Integration;

public sealed class AuthDatabaseInitializer(
    AuthDbContext domainDb,
    AuthIntegrationDbContext integrationDb,
    IPasswordHasher passwordHasher,
    IConfiguration configuration)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await domainDb.Database.MigrateAsync(cancellationToken);
        await integrationDb.Database.MigrateAsync(cancellationToken);
        if (!await integrationDb.ServiceInstances.AnyAsync(cancellationToken))
        {
            integrationDb.ServiceInstances.Add(new ServiceInstanceModel
            {
                Id = configuration.GetValue<Guid>("Seed:DefaultServiceInstanceId"),
                Name = configuration["Seed:DefaultServiceInstanceName"] ?? "crm-local",
                ApiUrl = configuration["Seed:DefaultServiceInstanceUrl"] ?? "http://localhost:9040",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await integrationDb.SaveChangesAsync(cancellationToken);
        }
        await AuthDataSeeder.SeedAsync(domainDb, passwordHasher, configuration, cancellationToken);
    }
}
