using Microsoft.AspNetCore.Mvc;
using ModernCRM.Crm.Application.Commands;
using ModernCRM.Crm.Application.DTO;
using ModernCRM.Crm.Application.Handlers;
using ModernCRM.Crm.Application.Queries;

namespace ModernCRM.Crm.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public sealed class AccountsController : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<AccountDto>> GetAll([FromQuery] string tenantId, [FromQuery] string? search, [FromServices] GetAccountsHandler handler, CancellationToken ct)
        => handler.Handle(new GetAccountsQuery(tenantId, search), ct);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, [FromServices] GetAccountByIdHandler handler, CancellationToken ct)
        => (await handler.Handle(new GetAccountByIdQuery(id), ct)) is { } item ? Ok(item) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountCommand command, [FromServices] CreateAccountHandler handler, CancellationToken ct)
        => Created($"/api/accounts/{await handler.Handle(command, ct)}", null);

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAccountCommand command, [FromServices] UpdateAccountHandler handler, CancellationToken ct)
        => await handler.Handle(command with { Id = id }, ct) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, [FromServices] DeleteAccountHandler handler, CancellationToken ct)
        => await handler.Handle(new DeleteAccountCommand(id), ct) ? NoContent() : NotFound();
}