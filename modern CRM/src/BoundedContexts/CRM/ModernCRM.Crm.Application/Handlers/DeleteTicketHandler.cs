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

public sealed class DeleteTicketHandler : ICommandHandler<DeleteTicketCommand, bool>
{
    private readonly ITicketRepository _tickets;
    public DeleteTicketHandler(ITicketRepository tickets) => _tickets = tickets;

    public async Task<bool> Handle(DeleteTicketCommand command, CancellationToken ct)
    {
        var ticket = await _tickets.GetAsync(TenantId.Create(command.TenantId), command.Id, ct);
        if (ticket is null) return false;
        ticket.Remove();
        await _tickets.UnitOfWork.SaveChangesAsync(ct);
        return true;
    }
}
