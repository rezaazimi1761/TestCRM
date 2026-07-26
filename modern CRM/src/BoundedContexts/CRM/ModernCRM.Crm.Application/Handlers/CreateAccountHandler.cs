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

public sealed class CreateAccountHandler : ICommandHandler<CreateAccountCommand, int>
{
    private readonly IAccountRepository _accounts;
    public CreateAccountHandler(IAccountRepository accounts) => _accounts = accounts;

    public async Task<int> Handle(CreateAccountCommand command, CancellationToken ct)
    {
        var account = Account.Create(TenantId.Create(command.TenantId), command.Name, command.Industry, command.Website);
        account.ChangeProfile(command.Industry, command.Website, command.Phone, command.Address);
        await _accounts.AddAsync(account, ct);
        await _accounts.SaveChangesAsync(ct);
        return account.Id;
    }
}
