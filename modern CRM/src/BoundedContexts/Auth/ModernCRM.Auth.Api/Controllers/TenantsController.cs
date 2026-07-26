using Microsoft.AspNetCore.Mvc;
using ModernCRM.Auth.Application.Commands;
using ModernCRM.Auth.Application.Handlers;
using ModernCRM.Auth.Application.Queries;

namespace ModernCRM.Auth.Api.Controllers;

[ApiController]
[Route("api/tenants")]
public sealed class TenantsController : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ModernCRM.Auth.Application.DTO.TenantDto>> GetAll([FromServices] GetTenantsHandler handler, CancellationToken ct)
        => handler.Handle(new GetTenantsQuery(), ct);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, [FromServices] GetTenantByIdHandler handler, CancellationToken ct)
        => (await handler.Handle(new GetTenantByIdQuery(id), ct)) is { } tenant ? Ok(tenant) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantCommand command, [FromServices] CreateTenantHandler handler, CancellationToken ct)
        => Created($"/api/tenants/{await handler.Handle(command, ct)}", null);

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTenantCommand command, [FromServices] UpdateTenantHandler handler, CancellationToken ct)
        => await handler.Handle(command with { Id = id }, ct) ? NoContent() : NotFound();
}
