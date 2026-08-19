using ModernCRM.Crm.Domain.Accounts;
using ModernCRM.Crm.Domain.Contacts;
using ModernCRM.Crm.Domain.Opportunities;
using ModernCRM.Crm.Domain.Tickets;
using ModernCRM.Crm.Domain.Users;
using ModernCRM.SharedKernel.ValueObjects;
using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Domain.Repositories;

public interface IUserReferenceRepository
{
    Task UpsertAsync(CrmUserReference user, CancellationToken ct);
    IUnitOfWork UnitOfWork { get; }
}
