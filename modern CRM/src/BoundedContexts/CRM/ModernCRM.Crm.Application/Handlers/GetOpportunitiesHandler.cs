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

public sealed class GetOpportunitiesHandler : IQueryHandler<GetOpportunitiesQuery, IReadOnlyList<OpportunityDto>>
{
    private readonly IOpportunityRepository _opportunities;
    public GetOpportunitiesHandler(IOpportunityRepository opportunities) => _opportunities = opportunities;

    public async Task<IReadOnlyList<OpportunityDto>> Handle(GetOpportunitiesQuery query, CancellationToken ct)
    {
        var items = await _opportunities.ListAsync(TenantId.Create(query.TenantId), query.Stage, query.Search, ct);
        return items.Select(x => new OpportunityDto(x.Id, x.TenantId.Value, x.Title, x.Value.Amount, x.AccountId, x.ContactId, x.Stage.ToString(), x.ExpectedCloseDate)).ToList();
    }
}
