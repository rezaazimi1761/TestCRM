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

public sealed class GetTicketsHandler : IQueryHandler<GetTicketsQuery, IReadOnlyList<TicketDto>>
{
    private readonly ITicketRepository _tickets;
    public GetTicketsHandler(ITicketRepository tickets) => _tickets = tickets;

    public async Task<IReadOnlyList<TicketDto>> Handle(GetTicketsQuery query, CancellationToken ct)
    {
        var items = await _tickets.ListAsync(TenantId.Create(query.TenantId), query.Status, query.Priority, ct);
        return items.Select(x => new TicketDto(x.Id, x.TenantId.Value, x.Subject, x.Description, x.RequestedByAccountId, x.ContactId, x.AssignedToAuthUserId, x.Status.ToString(), x.Priority.ToString(), x.DueDate)).ToList();
    }
}
