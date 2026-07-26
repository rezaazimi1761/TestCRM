using ModernCRM.Crm.Domain.Accounts;
using ModernCRM.Crm.Domain.Contacts;
using ModernCRM.Crm.Domain.Opportunities;
using ModernCRM.Crm.Domain.Tickets;
using ModernCRM.Crm.Domain.Users;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Crm.Domain.Repositories;

public interface IAccountRepository
{
    Task AddAsync(Account account, CancellationToken ct);
    Task<Account?> GetAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<Account>> ListAsync(TenantId tenantId, string? search, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
