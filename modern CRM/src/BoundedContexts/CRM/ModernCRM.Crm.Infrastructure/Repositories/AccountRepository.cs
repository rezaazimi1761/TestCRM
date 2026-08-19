using ModernCRM.Crm.Domain.Accounts;
using ModernCRM.Crm.Domain.Repositories;
using ModernCRM.Crm.Infrastructure.Persistence;
using ModernCRM.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Infrastructure.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly CrmDbContext _db;
    public AccountRepository(CrmDbContext db) { _db = db; UnitOfWork = new EfUnitOfWork(db); }
    public IUnitOfWork UnitOfWork { get; }

    public async Task AddAsync(Account account, CancellationToken ct) => await _db.Accounts.AddAsync(account, ct);

    public Task<Account?> GetAsync(TenantId tenantId, int id, CancellationToken ct) => _db.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId && !a.IsDeleted, ct);

    public async Task<IReadOnlyList<Account>> ListAsync(TenantId tenantId, string? search, CancellationToken ct)
    {
        var query = _db.Accounts.Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Name.Contains(term) || (x.Industry != null && x.Industry.Contains(term)) || (x.Website != null && x.Website.Contains(term)));
        }
        return await query.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);
    }

}
