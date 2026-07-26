using Microsoft.AspNetCore.Mvc;
using ModernCRM.Crm.Application.Commands;
using ModernCRM.Crm.Application.DTO;
using ModernCRM.Crm.Application.Handlers;
using ModernCRM.Crm.Application.Queries;

namespace ModernCRM.Crm.Api.Controllers;

[ApiController]
[Route("api/opportunities")]
public sealed class OpportunitiesController : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<OpportunityDto>> GetAll([FromQuery] string tenantId, [FromQuery] string? stage, [FromQuery] string? search, [FromServices] GetOpportunitiesHandler handler, CancellationToken ct)
        => handler.Handle(new GetOpportunitiesQuery(tenantId, stage, search), ct);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, [FromServices] GetOpportunityByIdHandler handler, CancellationToken ct)
        => (await handler.Handle(new GetOpportunityByIdQuery(id), ct)) is { } item ? Ok(item) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOpportunityCommand command, [FromServices] CreateOpportunityHandler handler, CancellationToken ct)
        => Created($"/api/opportunities/{await handler.Handle(command, ct)}", null);

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOpportunityCommand command, [FromServices] UpdateOpportunityHandler handler, CancellationToken ct)
        => await handler.Handle(command with { Id = id }, ct) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, [FromServices] DeleteOpportunityHandler handler, CancellationToken ct)
        => await handler.Handle(new DeleteOpportunityCommand(id), ct) ? NoContent() : NotFound();
}