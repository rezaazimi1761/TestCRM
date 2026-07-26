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

public sealed class GetAccountByIdHandler : IQueryHandler<GetAccountByIdQuery, AccountDto?>
{
    private readonly IAccountRepository _accounts;
    public GetAccountByIdHandler(IAccountRepository accounts) => _accounts = accounts;

    public async Task<AccountDto?> Handle(GetAccountByIdQuery query, CancellationToken ct)
    {
        var x = await _accounts.GetAsync(query.Id, ct);
        return x is null || x.IsDeleted ? null : new AccountDto(x.Id, x.TenantId.Value, x.Name, x.Industry, x.Website, x.Phone, x.Address);
    }
}
