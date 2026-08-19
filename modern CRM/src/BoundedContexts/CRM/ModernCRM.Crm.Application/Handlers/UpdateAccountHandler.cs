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

public sealed class UpdateAccountHandler : ICommandHandler<UpdateAccountCommand, bool>
{
    private readonly IAccountRepository _accounts;
    public UpdateAccountHandler(IAccountRepository accounts) => _accounts = accounts;

    public async Task<bool> Handle(UpdateAccountCommand command, CancellationToken ct)
    {
        var account = await _accounts.GetAsync(TenantId.Create(command.TenantId), command.Id, ct);
        if (account is null) return false;
        account.Rename(command.Name);
        account.ChangeProfile(command.Industry, command.Website, command.Phone, command.Address);
        await _accounts.UnitOfWork.SaveChangesAsync(ct);
        return true;
    }
}
