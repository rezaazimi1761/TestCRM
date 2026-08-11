using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernCRM.Auth.Api.Services;
using ModernCRM.Auth.Api.UserSync;
using ModernCRM.Auth.Infrastructure.Persistence;

namespace ModernCRM.Auth.Api.Controllers;

[ApiController, Authorize(Roles = "SuperUser"), Route("api/service-instances")]
public sealed class ServiceInstancesController(AuthIntegrationDbContext integrationDb, AuthDbContext authDb) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await integrationDb.ServiceInstances.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);
        var tenants = await authDb.Tenants.AsNoTracking().Where(t => !t.IsDeleted).Select(t => t.ServiceInstanceId).ToListAsync(ct);
        foreach (var item in items) item.TenantCount = tenants.Count(id => id == item.Id);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        => await integrationDb.ServiceInstances.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct) is { } item ? Ok(item) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(ServiceInstancePayload request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.ApiUrl)) return BadRequest(new { message = "Name and API URL are required." });
        var item = new ServiceInstanceModel { Id = Guid.NewGuid(), Name = request.Name, ApiUrl = request.ApiUrl, Description = request.Description, IsActive = true, CreatedAt = DateTime.UtcNow };
        integrationDb.ServiceInstances.Add(item);
        await integrationDb.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item.Id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ServiceInstancePayload request, CancellationToken ct)
    {
        var item = await integrationDb.ServiceInstances.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item is null) return NotFound();
        item.Name = request.Name ?? item.Name; item.ApiUrl = request.ApiUrl ?? item.ApiUrl; item.Description = request.Description; item.IsActive = request.IsActive ?? item.IsActive;
        await integrationDb.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (await authDb.Tenants.AnyAsync(t => t.ServiceInstanceId == id && !t.IsDeleted, ct)) return Conflict(new { message = "Service instance has attached tenants." });
        var item = await integrationDb.ServiceInstances.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item is null) return NotFound();
        integrationDb.ServiceInstances.Remove(item);
        await integrationDb.SaveChangesAsync(ct);
        return NoContent();
    }
}
