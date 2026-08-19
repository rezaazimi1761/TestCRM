using Microsoft.AspNetCore.Mvc;
using ModernCRM.Crm.Api.Frontend;
using ModernCRM.Crm.Application.Commands;
using ModernCRM.Crm.Application.Handlers;
using ModernCRM.Crm.Application.Queries;

namespace ModernCRM.Crm.Api.Controllers;

[ApiController, Route("api/opportunities")]
public sealed class OpportunitiesController(CreateOpportunityHandler create, UpdateOpportunityHandler update, DeleteOpportunityHandler delete, GetOpportunitiesHandler list, GetOpportunityByIdHandler get) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 20, string? sortBy = null, bool sortDesc = false, string? search = null, string? stage = null, CancellationToken ct = default)
        => Ok(FrontendApi.Page(await list.Handle(new GetOpportunitiesQuery(Tenant(), stage, search), ct), page, pageSize, sortBy, sortDesc));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
        => await get.Handle(new GetOpportunityByIdQuery(Tenant(), id), ct) is { } item ? Ok(item) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(OpportunityPayload request, CancellationToken ct)
    {
        var id = await create.Handle(new CreateOpportunityCommand(Tenant(), request.Title ?? "", request.Value, request.AccountId, request.ContactId, request.Stage ?? "Prospecting", request.ExpectedCloseDate), ct);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, OpportunityPayload request, CancellationToken ct)
        => await update.Handle(new UpdateOpportunityCommand(Tenant(), id, request.Title ?? "", request.Value, request.ContactId, request.Stage ?? "Prospecting", request.ExpectedCloseDate), ct) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await delete.Handle(new DeleteOpportunityCommand(Tenant(), id), ct) ? NoContent() : NotFound();

    private string Tenant() => FrontendApi.Tenant(User);
}
