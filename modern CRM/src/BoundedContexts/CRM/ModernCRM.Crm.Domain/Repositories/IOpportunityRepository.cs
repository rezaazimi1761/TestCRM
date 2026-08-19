using ModernCRM.Crm.Domain.Accounts;
using ModernCRM.Crm.Domain.Contacts;
using ModernCRM.Crm.Domain.Opportunities;
using ModernCRM.Crm.Domain.Tickets;
using ModernCRM.Crm.Domain.Users;
using ModernCRM.SharedKernel.ValueObjects;
using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Domain.Repositories;

public interface IOpportunityRepository
{
    Task AddAsync(Opportunity opportunity, CancellationToken ct);
    Task<Opportunity?> GetAsync(TenantId tenantId, int id, CancellationToken ct);
    Task<IReadOnlyList<Opportunity>> ListAsync(TenantId tenantId, string? stage, string? search, CancellationToken ct);
    IUnitOfWork UnitOfWork { get; }
}
