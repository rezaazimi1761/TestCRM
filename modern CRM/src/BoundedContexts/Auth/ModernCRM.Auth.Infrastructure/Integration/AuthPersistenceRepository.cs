using Microsoft.EntityFrameworkCore;
using ModernCRM.Auth.Domain.Tenants;
using ModernCRM.Auth.Domain.Users;
using ModernCRM.Auth.Infrastructure.Persistence;
using ModernCRM.SharedKernel.Application;
using ModernCRM.SharedKernel.ValueObjects;
using ModernCRM.Auth.Application.Integration;
using ModernCRM.Auth.Domain.Roles;

namespace ModernCRM.Auth.Infrastructure.Integration;

public sealed class AuthPersistenceRepository : IAuthPersistenceRepository
{
    private readonly AuthDbContext _domainDb;
    private readonly AuthIntegrationDbContext _integrationDb;

    public AuthPersistenceRepository(AuthDbContext domainDb, AuthIntegrationDbContext integrationDb)
    {
        _domainDb = domainDb;
        _integrationDb = integrationDb;
        DomainUnitOfWork = new EfUnitOfWork(domainDb);
        IntegrationUnitOfWork = new EfUnitOfWork(integrationDb);
    }

    public IUnitOfWork DomainUnitOfWork { get; }
    public IUnitOfWork IntegrationUnitOfWork { get; }

    public Task<SyncedAuthUser?> FindSyncedUserAsync(string tenantId, string username, CancellationToken cancellationToken)
        => _integrationDb.Users.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Username == username && !x.IsDeleted && x.IsActive, cancellationToken);

    public Task<SyncedAuthUser?> FindSyncedUserAsync(int crmUserId, string tenantId, CancellationToken cancellationToken)
        => _integrationDb.Users.FirstOrDefaultAsync(x => x.CrmUserId == crmUserId && x.TenantId == tenantId, cancellationToken);

    public async Task<(IReadOnlyList<AuthUser> Items, int Total)> PageUsersAsync(string tenantId, int page, int pageSize, string? sortBy, bool sortDesc, string? search, string? role, CancellationToken cancellationToken)
    {
        page = Math.Max(page, 1); pageSize = Math.Clamp(pageSize < 1 ? 20 : pageSize, 1, 500);
        var tenant = TenantId.Create(tenantId);
        var query = _domainDb.Users.AsNoTracking().Where(x => x.TenantId == tenant && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Username.Value.Contains(search) || x.FirstName.Contains(search) || x.LastName.Contains(search) || x.Email.Value.Contains(search));
        if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<Role>(role, true, out var parsedRole))
            query = query.Where(x => x.Role == parsedRole);
        query = sortBy?.ToLowerInvariant() switch
        {
            "username" => sortDesc ? query.OrderByDescending(x => x.Username.Value) : query.OrderBy(x => x.Username.Value),
            "firstname" => sortDesc ? query.OrderByDescending(x => x.FirstName) : query.OrderBy(x => x.FirstName),
            "lastname" => sortDesc ? query.OrderByDescending(x => x.LastName) : query.OrderBy(x => x.LastName),
            "email" => sortDesc ? query.OrderByDescending(x => x.Email.Value) : query.OrderBy(x => x.Email.Value),
            "role" => sortDesc ? query.OrderByDescending(x => x.Role) : query.OrderBy(x => x.Role),
            _ => query.OrderByDescending(x => x.Id)
        };
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, total);
    }

    public Task<AuthUser?> FindUserAsync(string tenantId, int id, CancellationToken cancellationToken)
        => _domainDb.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == TenantId.Create(tenantId) && !x.IsDeleted, cancellationToken);

    public Task<List<Tenant>> ListTenantsAsync(CancellationToken cancellationToken)
        => _domainDb.Tenants.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync(cancellationToken);

    public Task<List<AuthUser>> ListActiveUsersAsync(CancellationToken cancellationToken)
        => _domainDb.Users.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync(cancellationToken);

    public Task<Tenant?> FindTenantAsync(string slug, bool tracking, CancellationToken cancellationToken)
    {
        var id = TenantId.Create(slug);
        var query = _domainDb.Tenants.Where(x => x.TenantId == id && !x.IsDeleted);
        return (tracking ? query : query.AsNoTracking()).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> TenantExistsAsync(string slug, CancellationToken cancellationToken)
        => _domainDb.Tenants.AnyAsync(x => x.TenantId == TenantId.Create(slug) && !x.IsDeleted, cancellationToken);

    public Task<List<ServiceInstanceModel>> ListServiceInstancesAsync(CancellationToken cancellationToken)
        => _integrationDb.ServiceInstances.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task<ServiceInstanceModel?> FindServiceInstanceAsync(Guid id, bool tracking, CancellationToken cancellationToken)
    {
        var query = _integrationDb.ServiceInstances.Where(x => x.Id == id);
        return (tracking ? query : query.AsNoTracking()).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> ActiveServiceInstanceExistsAsync(Guid id, CancellationToken cancellationToken)
        => _integrationDb.ServiceInstances.AnyAsync(x => x.Id == id && x.IsActive, cancellationToken);

    public Task<bool> ServiceInstanceHasTenantsAsync(Guid id, CancellationToken cancellationToken)
        => _domainDb.Tenants.AnyAsync(x => x.ServiceInstanceId == id && !x.IsDeleted, cancellationToken);

    public void Add(Tenant tenant) => _domainDb.Tenants.Add(tenant);
    public void Add(SyncedAuthUser user) => _integrationDb.Users.Add(user);
    public void Add(ServiceInstanceModel instance) => _integrationDb.ServiceInstances.Add(instance);
    public void Remove(ServiceInstanceModel instance) => _integrationDb.ServiceInstances.Remove(instance);
}
