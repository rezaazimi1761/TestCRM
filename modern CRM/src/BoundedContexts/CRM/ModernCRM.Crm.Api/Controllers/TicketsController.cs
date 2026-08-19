using Microsoft.AspNetCore.Mvc;
using ModernCRM.Crm.Api.Frontend;
using ModernCRM.Crm.Application.Commands;
using ModernCRM.Crm.Application.Handlers;
using ModernCRM.Crm.Application.Queries;

namespace ModernCRM.Crm.Api.Controllers;

[ApiController, Route("api/tickets")]
public sealed class TicketsController(CreateTicketHandler create, UpdateTicketHandler update, DeleteTicketHandler delete, GetTicketsHandler list, GetTicketByIdHandler get) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 20, string? sortBy = null, bool sortDesc = false, string? search = null, string? status = null, string? priority = null, CancellationToken ct = default)
        => Ok(FrontendApi.Page(await list.Handle(new GetTicketsQuery(Tenant(), status, priority), ct), page, pageSize, sortBy, sortDesc));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
        => await get.Handle(new GetTicketByIdQuery(Tenant(), id), ct) is { } item ? Ok(item) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(TicketPayload request, CancellationToken ct)
    {
        if (request.AccountId is not > 0) return BadRequest(new { message = "Account is required." });
        var id = await create.Handle(new CreateTicketCommand(Tenant(), request.Subject ?? "", request.AccountId.Value, request.Priority ?? "Medium", request.DueDate, request.Description, request.ContactId, request.AssignedToUserId), ct);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TicketPayload request, CancellationToken ct)
        => await update.Handle(new UpdateTicketCommand(Tenant(), id, request.Subject ?? "", request.Priority ?? "Medium", request.Status ?? "New", request.DueDate, request.Description, request.ContactId, request.AssignedToUserId), ct) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await delete.Handle(new DeleteTicketCommand(Tenant(), id), ct) ? NoContent() : NotFound();

    private string Tenant() => FrontendApi.Tenant(User);
}
