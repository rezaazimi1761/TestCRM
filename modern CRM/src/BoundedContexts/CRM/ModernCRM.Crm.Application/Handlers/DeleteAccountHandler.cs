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

public sealed class DeleteAccountHandler : ICommandHandler<DeleteAccountCommand, bool>
{
    private readonly IAccountRepository _accounts;
    public DeleteAccountHandler(IAccountRepository accounts) => _accounts = accounts;

    public async Task<bool> Handle(DeleteAccountCommand command, CancellationToken ct)
    {
        var account = await _accounts.GetAsync(command.Id, ct);
        if (account is null) return false;
        account.Delete();
        await _accounts.SaveChangesAsync(ct);
        return true;
    }
}
