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

public sealed class CreateOpportunityHandler : ICommandHandler<CreateOpportunityCommand, int>
{
    private readonly IOpportunityRepository _opportunities;
    public CreateOpportunityHandler(IOpportunityRepository opportunities) => _opportunities = opportunities;

    public async Task<int> Handle(CreateOpportunityCommand command, CancellationToken ct)
    {
        var opportunity = Opportunity.Create(TenantId.Create(command.TenantId), command.Title, Money.Create(command.Value), command.AccountId, command.ExpectedCloseDate);
        if (command.ContactId is > 0) opportunity.LinkContact(command.ContactId.Value);
        if (Enum.TryParse<OpportunityStage>(command.Stage, true, out var stage) && stage != OpportunityStage.Prospecting)
            opportunity.MoveTo(stage);
        await _opportunities.AddAsync(opportunity, ct);
        await _opportunities.SaveChangesAsync(ct);
        return opportunity.Id;
    }
}
