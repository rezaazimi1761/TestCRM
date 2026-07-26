using Microsoft.AspNetCore.Mvc;
using ModernCRM.Crm.Application.Commands;
using ModernCRM.Crm.Application.DTO;
using ModernCRM.Crm.Application.Handlers;
using ModernCRM.Crm.Application.Queries;

namespace ModernCRM.Crm.Api.Controllers;

[ApiController]
[Route("api/contacts")]
public sealed class ContactsController : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ContactDto>> GetAll([FromQuery] string tenantId, [FromQuery] string? search, [FromServices] GetContactsHandler handler, CancellationToken ct)
        => handler.Handle(new GetContactsQuery(tenantId, search), ct);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, [FromServices] GetContactByIdHandler handler, CancellationToken ct)
        => (await handler.Handle(new GetContactByIdQuery(id), ct)) is { } item ? Ok(item) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContactCommand command, [FromServices] CreateContactHandler handler, CancellationToken ct)
        => Created($"/api/contacts/{await handler.Handle(command, ct)}", null);

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateContactCommand command, [FromServices] UpdateContactHandler handler, CancellationToken ct)
        => await handler.Handle(command with { Id = id }, ct) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, [FromServices] DeleteContactHandler handler, CancellationToken ct)
        => await handler.Handle(new DeleteContactCommand(id), ct) ? NoContent() : NotFound();
}