using ModernCRM.Crm.Application.Commands;
using ModernCRM.Crm.Application.DTO;
using ModernCRM.Crm.Application.Queries;
using ModernCRM.Crm.Domain.Accounts;
using ModernCRM.Crm.Domain.Contacts;
using ModernCRM.Crm.Domain.Opportunities;
using ModernCRM.Crm.Domain.Repositories;
using ModernCRM.Crm.Domain.Tickets;
using ModernCRM.Crm.Domain.ValueObjects;
using ModernCRM.SharedKernel.Application;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Crm.Application.Handlers;

public sealed class DeleteOpportunityHandler : ICommandHandler<DeleteOpportunityCommand, bool>
{
    private readonly IOpportunityRepository _opportunities;
    public DeleteOpportunityHandler(IOpportunityRepository opportunities) => _opportunities = opportunities;

    public async Task<bool> Handle(DeleteOpportunityCommand command, CancellationToken ct)
    {
        var opportunity = await _opportunities.GetAsync(TenantId.Create(command.TenantId), command.Id, ct);
        if (opportunity is null) return false;
        opportunity.Delete();
        await _opportunities.UnitOfWork.SaveChangesAsync(ct);
        return true;
    }
}
