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

public sealed class CreateContactHandler : ICommandHandler<CreateContactCommand, int>
{
    private readonly IContactRepository _contacts;
    public CreateContactHandler(IContactRepository contacts) => _contacts = contacts;

    public async Task<int> Handle(CreateContactCommand command, CancellationToken ct)
    {
        var contact = Contact.Create(TenantId.Create(command.TenantId), command.FirstName, command.LastName, Email.Create(command.Email), command.Phone);
        contact.ChangeJobTitle(command.JobTitle);
        if (command.AccountId is > 0) contact.AssignToAccount(command.AccountId.Value);
        await _contacts.AddAsync(contact, ct);
        await _contacts.SaveChangesAsync(ct);
        return contact.Id;
    }
}
