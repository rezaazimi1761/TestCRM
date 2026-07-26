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

public sealed class GetContactsHandler : IQueryHandler<GetContactsQuery, IReadOnlyList<ContactDto>>
{
    private readonly IContactRepository _contacts;
    public GetContactsHandler(IContactRepository contacts) => _contacts = contacts;

    public async Task<IReadOnlyList<ContactDto>> Handle(GetContactsQuery query, CancellationToken ct)
    {
        var items = await _contacts.ListAsync(TenantId.Create(query.TenantId), query.Search, ct);
        return items.Select(x => new ContactDto(x.Id, x.TenantId.Value, x.FirstName, x.LastName, x.Email.Value, x.Phone, x.JobTitle, x.AccountId)).ToList();
    }
}
