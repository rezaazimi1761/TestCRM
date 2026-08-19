using Microsoft.EntityFrameworkCore;
using ModernCRM.Crm.Infrastructure.Integration;
using ModernCRM.Crm.Infrastructure.Persistence;

namespace ModernCRM.Crm.Infrastructure.Integration;

public sealed class CrmDatabaseInitializer(CrmDbContext domainDb, CrmIntegrationDbContext integrationDb)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await domainDb.Database.MigrateAsync(cancellationToken);
        await integrationDb.Database.MigrateAsync(cancellationToken);
    }
}
