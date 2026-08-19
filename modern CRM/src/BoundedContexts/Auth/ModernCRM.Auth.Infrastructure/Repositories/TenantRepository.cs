using ModernCRM.Auth.Application.Handlers;
using ModernCRM.Auth.Domain.Tenants;
using ModernCRM.Auth.Infrastructure.Persistence;
using ModernCRM.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Auth.Infrastructure.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly AuthDbContext _db;
    public TenantRepository(AuthDbContext db) { _db = db; UnitOfWork = new EfUnitOfWork(db); }
    public IUnitOfWork UnitOfWork { get; }

    public async Task AddAsync(Tenant tenant, CancellationToken ct) => await _db.Tenants.AddAsync(tenant, ct);

    public Task<Tenant?> GetByIdAsync(int id, CancellationToken ct) => _db.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);
    public Task<Tenant?> GetByTenantIdAsync(TenantId tenantId, CancellationToken ct) => _db.Tenants.FirstOrDefaultAsync(t => t.TenantId == tenantId, ct);
    public async Task<IReadOnlyList<Tenant>> ListAsync(CancellationToken ct) => await _db.Tenants.AsNoTracking().OrderBy(t => t.Id).ToListAsync(ct);
}
