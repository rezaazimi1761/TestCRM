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

public sealed class DeleteContactHandler : ICommandHandler<DeleteContactCommand, bool>
{
    private readonly IContactRepository _contacts;
    public DeleteContactHandler(IContactRepository contacts) => _contacts = contacts;

    public async Task<bool> Handle(DeleteContactCommand command, CancellationToken ct)
    {
        var contact = await _contacts.GetAsync(TenantId.Create(command.TenantId), command.Id, ct);
        if (contact is null) return false;
        contact.Delete();
        await _contacts.UnitOfWork.SaveChangesAsync(ct);
        return true;
    }
}
