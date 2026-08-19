using Microsoft.AspNetCore.Mvc;
using ModernCRM.Crm.Api.Frontend;
using ModernCRM.Crm.Application.Commands;
using ModernCRM.Crm.Application.Handlers;
using ModernCRM.Crm.Application.Queries;

namespace ModernCRM.Crm.Api.Controllers;

[ApiController, Route("api/accounts")]
public sealed class AccountsController(CreateAccountHandler create, UpdateAccountHandler update, DeleteAccountHandler delete, GetAccountsHandler list, GetAccountByIdHandler get) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 20, string? sortBy = null, bool sortDesc = false, string? search = null, CancellationToken ct = default)
        => Ok(FrontendApi.Page(await list.Handle(new GetAccountsQuery(Tenant(), search), ct), page, pageSize, sortBy, sortDesc));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
        => await get.Handle(new GetAccountByIdQuery(Tenant(), id), ct) is { } item ? Ok(item) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(AccountPayload request, CancellationToken ct)
    {
        var id = await create.Handle(new CreateAccountCommand(Tenant(), request.Name ?? "", request.Industry, request.Website, request.Phone, request.Address), ct);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AccountPayload request, CancellationToken ct)
        => await update.Handle(new UpdateAccountCommand(Tenant(), id, request.Name ?? "", request.Industry, request.Website, request.Phone, request.Address), ct) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await delete.Handle(new DeleteAccountCommand(Tenant(), id), ct) ? NoContent() : NotFound();

    private string Tenant() => FrontendApi.Tenant(User);
}
