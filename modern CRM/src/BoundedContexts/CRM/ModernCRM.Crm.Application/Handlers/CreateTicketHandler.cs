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

public sealed class CreateTicketHandler : ICommandHandler<CreateTicketCommand, int>
{
    private readonly ITicketRepository _tickets;
    public CreateTicketHandler(ITicketRepository tickets) => _tickets = tickets;

    public async Task<int> Handle(CreateTicketCommand command, CancellationToken ct)
    {
        var priority = Enum.Parse<TicketPriority>(command.Priority, true);
        var ticket = Ticket.Create(TenantId.Create(command.TenantId), command.AccountId, command.Subject, priority, command.DueDate);
        ticket.ChangeDescription(command.Description);
        if (command.ContactId is > 0) ticket.LinkContact(command.ContactId.Value);
        if (command.AssignedToAuthUserId is > 0) ticket.AssignToUser(command.AssignedToAuthUserId.Value);
        await _tickets.AddAsync(ticket, ct);
        await _tickets.SaveChangesAsync(ct);
        return ticket.Id;
    }
}
