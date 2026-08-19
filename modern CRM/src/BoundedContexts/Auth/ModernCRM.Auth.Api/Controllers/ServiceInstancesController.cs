using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernCRM.Auth.Application.Integration;

namespace ModernCRM.Auth.Api.Controllers;

[ApiController, Authorize(Roles = "SuperUser"), Route("api/service-instances")]
public sealed class ServiceInstancesController(IAuthPersistenceRepository persistence) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await persistence.ListServiceInstancesAsync(ct);
        var tenants = await persistence.ListTenantsAsync(ct);
        foreach (var item in items) item.TenantCount = tenants.Count(tenant => tenant.ServiceInstanceId == item.Id);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        => await persistence.FindServiceInstanceAsync(id, false, ct) is { } item ? Ok(item) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(ServiceInstancePayload request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.ApiUrl)) return BadRequest(new { message = "Name and API URL are required." });
        var item = new ServiceInstanceModel { Id = Guid.NewGuid(), Name = request.Name, ApiUrl = request.ApiUrl, Description = request.Description, IsActive = true, CreatedAt = DateTime.UtcNow };
        persistence.Add(item);
        await persistence.IntegrationUnitOfWork.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item.Id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ServiceInstancePayload request, CancellationToken ct)
    {
        var item = await persistence.FindServiceInstanceAsync(id, true, ct);
        if (item is null) return NotFound();
        item.Name = request.Name ?? item.Name; item.ApiUrl = request.ApiUrl ?? item.ApiUrl; item.Description = request.Description; item.IsActive = request.IsActive ?? item.IsActive;
        await persistence.IntegrationUnitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (await persistence.ServiceInstanceHasTenantsAsync(id, ct)) return Conflict(new { message = "Service instance has attached tenants." });
        var item = await persistence.FindServiceInstanceAsync(id, true, ct);
        if (item is null) return NotFound();
        persistence.Remove(item);
        await persistence.IntegrationUnitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }
}
