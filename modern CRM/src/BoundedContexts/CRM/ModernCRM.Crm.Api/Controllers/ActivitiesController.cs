using Microsoft.AspNetCore.Mvc;
using ModernCRM.Crm.Api.Frontend;
using ModernCRM.Crm.Application.Frontend;

namespace ModernCRM.Crm.Api.Controllers;

[ApiController, Route("api/activities")]
public sealed class ActivitiesController(IActivityService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 20, string? sortBy = null, bool sortDesc = false, string? search = null, string? type = null, string? isCompleted = null, CancellationToken ct = default)
        => Ok(await service.GetPageAsync(Tenant(), page, pageSize, sortBy, sortDesc, search, type, bool.TryParse(isCompleted, out var completed) ? completed : null, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
        => await service.GetAsync(Tenant(), id, ct) is { } item ? Ok(item) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(ActivityPayload request, CancellationToken ct)
    {
        var id = await service.CreateAsync(Tenant(), ToInput(request), ct);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ActivityPayload request, CancellationToken ct)
        => await service.UpdateAsync(Tenant(), id, ToInput(request), ct) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await service.DeleteAsync(Tenant(), id, ct) ? NoContent() : NotFound();

    private string Tenant() => FrontendApi.Tenant(User);
    private static ActivityInput ToInput(ActivityPayload request) => new(request.Subject, request.Type, request.Description, request.DueDate, request.IsCompleted, request.ContactId);
}
