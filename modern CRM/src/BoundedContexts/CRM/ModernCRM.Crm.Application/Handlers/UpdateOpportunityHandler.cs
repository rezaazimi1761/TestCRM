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

public sealed class UpdateOpportunityHandler : ICommandHandler<UpdateOpportunityCommand, bool>
{
    private readonly IOpportunityRepository _opportunities;
    public UpdateOpportunityHandler(IOpportunityRepository opportunities) => _opportunities = opportunities;

    public async Task<bool> Handle(UpdateOpportunityCommand command, CancellationToken ct)
    {
        var opportunity = await _opportunities.GetAsync(TenantId.Create(command.TenantId), command.Id, ct);
        if (opportunity is null) return false;
        opportunity.Rename(command.Title);
        opportunity.ChangeValue(Money.Create(command.Value));
        opportunity.ChangeExpectedCloseDate(command.ExpectedCloseDate);
        if (command.ContactId is > 0) opportunity.LinkContact(command.ContactId.Value);
        if (Enum.TryParse<OpportunityStage>(command.Stage, true, out var stage) && stage != opportunity.Stage)
            opportunity.MoveTo(stage);
        await _opportunities.UnitOfWork.SaveChangesAsync(ct);
        return true;
    }
}
