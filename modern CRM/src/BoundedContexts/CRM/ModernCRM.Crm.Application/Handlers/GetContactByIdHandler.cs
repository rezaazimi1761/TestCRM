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

public sealed class GetContactByIdHandler : IQueryHandler<GetContactByIdQuery, ContactDto?>
{
    private readonly IContactRepository _contacts;
    public GetContactByIdHandler(IContactRepository contacts) => _contacts = contacts;

    public async Task<ContactDto?> Handle(GetContactByIdQuery query, CancellationToken ct)
    {
        var x = await _contacts.GetAsync(TenantId.Create(query.TenantId), query.Id, ct);
        return x is null || x.IsDeleted ? null : new ContactDto(x.Id, x.TenantId.Value, x.FirstName, x.LastName, x.Email.Value, x.Phone, x.JobTitle, x.AccountId);
    }
}
