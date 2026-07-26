using Microsoft.AspNetCore.Mvc;
using ModernCRM.Crm.Application.Commands;
using ModernCRM.Crm.Application.DTO;
using ModernCRM.Crm.Application.Handlers;
using ModernCRM.Crm.Application.Queries;

namespace ModernCRM.Crm.Api.Controllers;

[ApiController]
[Route("api/tickets")]
public sealed class TicketsController : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<TicketDto>> GetAll([FromQuery] string tenantId, [FromQuery] string? status, [FromQuery] string? priority, [FromServices] GetTicketsHandler handler, CancellationToken ct)
        => handler.Handle(new GetTicketsQuery(tenantId, status, priority), ct);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, [FromServices] GetTicketByIdHandler handler, CancellationToken ct)
        => (await handler.Handle(new GetTicketByIdQuery(id), ct)) is { } item ? Ok(item) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketCommand command, [FromServices] CreateTicketHandler handler, CancellationToken ct)
        => Created($"/api/tickets/{await handler.Handle(command, ct)}", null);

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTicketCommand command, [FromServices] UpdateTicketHandler handler, CancellationToken ct)
        => await handler.Handle(command with { Id = id }, ct) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, [FromServices] DeleteTicketHandler handler, CancellationToken ct)
        => await handler.Handle(new DeleteTicketCommand(id), ct) ? NoContent() : NotFound();
}