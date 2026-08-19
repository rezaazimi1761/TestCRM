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

public sealed class GetTicketByIdHandler : IQueryHandler<GetTicketByIdQuery, TicketDto?>
{
    private readonly ITicketRepository _tickets;
    public GetTicketByIdHandler(ITicketRepository tickets) => _tickets = tickets;

    public async Task<TicketDto?> Handle(GetTicketByIdQuery query, CancellationToken ct)
    {
        var x = await _tickets.GetAsync(TenantId.Create(query.TenantId), query.Id, ct);
        return x is null || x.IsDeleted ? null : new TicketDto(x.Id, x.TenantId.Value, x.Subject, x.Description, x.RequestedByAccountId, x.ContactId, x.AssignedToAuthUserId, x.Status.ToString(), x.Priority.ToString(), x.DueDate);
    }
}
