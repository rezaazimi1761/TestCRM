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

public interface IContactRepository
{
    Task AddAsync(Contact contact, CancellationToken ct);
    Task<Contact?> GetAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<Contact>> ListAsync(TenantId tenantId, string? search, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public interface ITicketRepository
{
    Task AddAsync(Ticket ticket, CancellationToken ct);
    Task<Ticket?> GetAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<Ticket>> ListAsync(TenantId tenantId, string? status, string? priority, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public interface IOpportunityRepository
{
    Task AddAsync(Opportunity opportunity, CancellationToken ct);
    Task<Opportunity?> GetAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<Opportunity>> ListAsync(TenantId tenantId, string? stage, string? search, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public interface IUserReferenceRepository
{
    Task UpsertAsync(CrmUserReference user, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}