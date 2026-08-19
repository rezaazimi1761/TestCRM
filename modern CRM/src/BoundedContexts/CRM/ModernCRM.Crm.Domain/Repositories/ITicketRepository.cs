using ModernCRM.Crm.Domain.Accounts;
using ModernCRM.Crm.Domain.Contacts;
using ModernCRM.Crm.Domain.Opportunities;
using ModernCRM.Crm.Domain.Tickets;
using ModernCRM.Crm.Domain.Users;
using ModernCRM.SharedKernel.ValueObjects;
using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Domain.Repositories;

public interface ITicketRepository
{
    Task AddAsync(Ticket ticket, CancellationToken ct);
    Task<Ticket?> GetAsync(TenantId tenantId, int id, CancellationToken ct);
    Task<IReadOnlyList<Ticket>> ListAsync(TenantId tenantId, string? status, string? priority, CancellationToken ct);
    IUnitOfWork UnitOfWork { get; }
}
