using ModernCRM.Auth.Application.Handlers;
using ModernCRM.Auth.Domain.Tenants;
using ModernCRM.Auth.Infrastructure.Persistence;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Auth.Infrastructure.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly AuthDbContext _db;
    public TenantRepository(AuthDbContext db) => _db = db;

    public Task AddAsync(Tenant tenant, CancellationToken ct)
    {
        if (tenant.Id == 0) tenant.GetType().GetProperty("Id")!.SetValue(tenant, _db.NextTenantId());
        _db.Tenants.Add(tenant);
        return Task.CompletedTask;
    }

    public Task<Tenant?> GetByIdAsync(int id, CancellationToken ct) => Task.FromResult(_db.Tenants.FirstOrDefault(t => t.Id == id));
    public Task<Tenant?> GetByTenantIdAsync(TenantId tenantId, CancellationToken ct) => Task.FromResult(_db.Tenants.FirstOrDefault(t => t.TenantId == tenantId));
    public Task<IReadOnlyList<Tenant>> ListAsync(CancellationToken ct) => Task.FromResult<IReadOnlyList<Tenant>>(_db.Tenants.ToList());
    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
