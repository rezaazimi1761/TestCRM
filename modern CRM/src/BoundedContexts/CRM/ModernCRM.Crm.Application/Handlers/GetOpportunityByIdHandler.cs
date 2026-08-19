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

public sealed class GetOpportunityByIdHandler : IQueryHandler<GetOpportunityByIdQuery, OpportunityDto?>
{
    private readonly IOpportunityRepository _opportunities;
    public GetOpportunityByIdHandler(IOpportunityRepository opportunities) => _opportunities = opportunities;

    public async Task<OpportunityDto?> Handle(GetOpportunityByIdQuery query, CancellationToken ct)
    {
        var x = await _opportunities.GetAsync(TenantId.Create(query.TenantId), query.Id, ct);
        return x is null || x.IsDeleted ? null : new OpportunityDto(x.Id, x.TenantId.Value, x.Title, x.Value.Amount, x.AccountId, x.ContactId, x.Stage.ToString(), x.ExpectedCloseDate);
    }
}
