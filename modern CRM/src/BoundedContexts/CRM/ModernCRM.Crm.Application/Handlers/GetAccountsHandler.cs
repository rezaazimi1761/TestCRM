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

public sealed class GetAccountsHandler : IQueryHandler<GetAccountsQuery, IReadOnlyList<AccountDto>>
{
    private readonly IAccountRepository _accounts;
    public GetAccountsHandler(IAccountRepository accounts) => _accounts = accounts;

    public async Task<IReadOnlyList<AccountDto>> Handle(GetAccountsQuery query, CancellationToken ct)
    {
        var items = await _accounts.ListAsync(TenantId.Create(query.TenantId), query.Search, ct);
        return items.Select(x => new AccountDto(x.Id, x.TenantId.Value, x.Name, x.Industry, x.Website, x.Phone, x.Address)).ToList();
    }
}
