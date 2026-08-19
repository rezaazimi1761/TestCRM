using Microsoft.AspNetCore.Mvc;
using ModernCRM.Crm.Api.Frontend;
using ModernCRM.Crm.Application.Commands;
using ModernCRM.Crm.Application.Handlers;
using ModernCRM.Crm.Application.Queries;

namespace ModernCRM.Crm.Api.Controllers;

[ApiController, Route("api/contacts")]
public sealed class ContactsController(CreateContactHandler create, UpdateContactHandler update, DeleteContactHandler delete, GetContactsHandler list, GetContactByIdHandler get) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 20, string? sortBy = null, bool sortDesc = false, string? search = null, CancellationToken ct = default)
        => Ok(FrontendApi.Page(await list.Handle(new GetContactsQuery(Tenant(), search), ct), page, pageSize, sortBy, sortDesc));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
        => await get.Handle(new GetContactByIdQuery(Tenant(), id), ct) is { } item ? Ok(item) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(ContactPayload request, CancellationToken ct)
    {
        var id = await create.Handle(new CreateContactCommand(Tenant(), request.FirstName ?? "", request.LastName ?? "", request.Email ?? "", request.Phone, request.JobTitle, request.AccountId), ct);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ContactPayload request, CancellationToken ct)
        => await update.Handle(new UpdateContactCommand(Tenant(), id, request.FirstName ?? "", request.LastName ?? "", request.Email ?? "", request.Phone, request.JobTitle, request.AccountId), ct) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await delete.Handle(new DeleteContactCommand(Tenant(), id), ct) ? NoContent() : NotFound();

    private string Tenant() => FrontendApi.Tenant(User);
}
