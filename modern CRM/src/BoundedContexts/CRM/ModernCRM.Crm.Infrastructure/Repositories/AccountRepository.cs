using ModernCRM.Crm.Domain.Accounts;
using ModernCRM.Crm.Domain.Repositories;
using ModernCRM.Crm.Infrastructure.Persistence;
using ModernCRM.SharedKernel.BuildingBlocks;
using ModernCRM.SharedKernel.ValueObjects;
using System.Reflection;

namespace ModernCRM.Crm.Infrastructure.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly CrmDbContext _db;
    public AccountRepository(CrmDbContext db) => _db = db;

    public Task AddAsync(Account account, CancellationToken ct)
    {
        if (account.Id == 0) SetId(account, _db.NextAccountId());
        _db.Accounts.Add(account);
        return Task.CompletedTask;
    }

    public Task<Account?> GetAsync(int id, CancellationToken ct) => Task.FromResult(_db.Accounts.FirstOrDefault(a => a.Id == id));

    public Task<IReadOnlyList<Account>> ListAsync(TenantId tenantId, string? search, CancellationToken ct)
    {
        IEnumerable<Account> query = _db.Accounts.Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (x.Industry?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Website?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        return Task.FromResult<IReadOnlyList<Account>>(query.OrderBy(x => x.Name).ToList());
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

    private static void SetId<TEntity>(TEntity entity, int id) where TEntity : Entity<int>
        => typeof(Entity<int>).GetProperty(nameof(Entity<int>.Id), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(entity, id);
}