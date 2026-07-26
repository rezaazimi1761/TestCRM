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

public sealed class UpdateTicketHandler : ICommandHandler<UpdateTicketCommand, bool>
{
    private readonly ITicketRepository _tickets;
    public UpdateTicketHandler(ITicketRepository tickets) => _tickets = tickets;

    public async Task<bool> Handle(UpdateTicketCommand command, CancellationToken ct)
    {
        var ticket = await _tickets.GetAsync(command.Id, ct);
        if (ticket is null) return false;

        ticket.ChangeSubject(command.Subject);
        ticket.ChangeDescription(command.Description);
        ticket.ChangePriority(Enum.Parse<TicketPriority>(command.Priority, true));
        if (command.ContactId is > 0) ticket.LinkContact(command.ContactId.Value);
        if (command.AssignedToAuthUserId is > 0) ticket.AssignToUser(command.AssignedToAuthUserId.Value);

        if (Enum.TryParse<TicketStatus>(command.Status, true, out var status))
        {
            if (status == TicketStatus.Active && ticket.Status == TicketStatus.New) ticket.Activate();
            else if (status == TicketStatus.Closed && ticket.Status != TicketStatus.Closed) ticket.Close();
            else if (status == TicketStatus.Removed) ticket.Remove();
        }

        await _tickets.SaveChangesAsync(ct);
        return true;
    }
}
