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

public sealed class UpdateContactHandler : ICommandHandler<UpdateContactCommand, bool>
{
    private readonly IContactRepository _contacts;
    public UpdateContactHandler(IContactRepository contacts) => _contacts = contacts;

    public async Task<bool> Handle(UpdateContactCommand command, CancellationToken ct)
    {
        var contact = await _contacts.GetAsync(command.Id, ct);
        if (contact is null) return false;
        contact.ChangeName(command.FirstName, command.LastName);
        contact.ChangeEmail(Email.Create(command.Email));
        contact.ChangePhone(command.Phone);
        contact.ChangeJobTitle(command.JobTitle);
        if (command.AccountId is > 0) contact.AssignToAccount(command.AccountId.Value);
        await _contacts.SaveChangesAsync(ct);
        return true;
    }
}
