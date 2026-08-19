using Microsoft.AspNetCore.Mvc;
using ModernCRM.Crm.Api.Frontend;
using ModernCRM.Crm.Application.Frontend;

namespace ModernCRM.Crm.Api.Controllers;

[ApiController, Route("api/leads")]
public sealed class LeadsController(ILeadService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 20, string? sortBy = null, bool sortDesc = false, string? search = null, string? status = null, CancellationToken ct = default)
        => Ok(await service.GetPageAsync(Tenant(), page, pageSize, sortBy, sortDesc, search, status, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
        => await service.GetAsync(Tenant(), id, ct) is { } item ? Ok(item) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(LeadPayload request, CancellationToken ct)
    {
        var id = await service.CreateAsync(Tenant(), ToInput(request), ct);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, LeadPayload request, CancellationToken ct)
        => await service.UpdateAsync(Tenant(), id, ToInput(request), ct) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await service.DeleteAsync(Tenant(), id, ct) ? NoContent() : NotFound();

    private string Tenant() => FrontendApi.Tenant(User);
    private static LeadInput ToInput(LeadPayload request) => new(request.FirstName, request.LastName, request.Email, request.Phone, request.Company, request.Status, request.Source, request.Notes);
}
