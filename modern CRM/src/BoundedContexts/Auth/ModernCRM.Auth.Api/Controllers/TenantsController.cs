using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernCRM.Auth.Api.UserSync;
using ModernCRM.Auth.Domain.Tenants;
using ModernCRM.Auth.Infrastructure.Persistence;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Auth.Api.Controllers;

[ApiController, Authorize, Route("api/tenants")]
public sealed class TenantsController(AuthDbContext db, AuthIntegrationDbContext integrationDb) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tenants = await db.Tenants.AsNoTracking().Where(t => !t.IsDeleted).ToListAsync(ct);
        var users = await db.Users.AsNoTracking().Where(u => !u.IsDeleted).ToListAsync(ct);
        var instances = await integrationDb.ServiceInstances.AsNoTracking().ToDictionaryAsync(i => i.Id, ct);
        return Ok(tenants.Select(t =>
        {
            instances.TryGetValue(t.ServiceInstanceId, out var instance);
            return new TenantView(t.Id, t.TenantId.Value, t.DisplayName, null, t.IsActive, DateTime.UtcNow, users.Count(u => u.TenantId == t.TenantId), t.ServiceInstanceId, instance?.Name, instance?.ApiUrl);
        }));
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Get(string slug, CancellationToken ct)
        => await db.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == TenantId.Create(slug) && !x.IsDeleted, ct) is { } tenant ? Ok(tenant) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(TenantPayload request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Slug) || string.IsNullOrWhiteSpace(request.DisplayName) || request.ServiceInstanceId == Guid.Empty) return BadRequest(new { message = "Slug, display name and service instance are required." });
        var tenantId = TenantId.Create(request.Slug);
        if (await db.Tenants.AnyAsync(x => x.TenantId == tenantId && !x.IsDeleted, ct)) return Conflict(new { message = "Tenant already exists." });
        if (!await integrationDb.ServiceInstances.AnyAsync(x => x.Id == request.ServiceInstanceId && x.IsActive, ct)) return BadRequest(new { message = "Service instance does not exist or is inactive." });
        var tenant = Tenant.Create(tenantId, request.DisplayName, request.ServiceInstanceId);
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { slug = tenant.TenantId.Value }, tenant.Id);
    }

    [HttpPut("{slug}")]
    public async Task<IActionResult> Update(string slug, TenantPayload request, CancellationToken ct)
    {
        var tenant = await Find(slug, ct); if (tenant is null) return NotFound();
        if (!string.IsNullOrWhiteSpace(request.DisplayName)) tenant.Rename(request.DisplayName);
        if (request.ServiceInstanceId != Guid.Empty) tenant.MoveToServiceInstance(request.ServiceInstanceId);
        if (request.IsActive == true) tenant.Activate(); else if (request.IsActive == false) tenant.Deactivate();
        await db.SaveChangesAsync(ct); return NoContent();
    }
    [HttpPost("{slug}/activate")] public async Task<IActionResult> Activate(string slug, CancellationToken ct) { var t = await Find(slug, ct); if (t is null) return NotFound(); t.Activate(); await db.SaveChangesAsync(ct); return NoContent(); }
    [HttpPost("{slug}/deactivate")] public async Task<IActionResult> Deactivate(string slug, CancellationToken ct) { var t = await Find(slug, ct); if (t is null) return NotFound(); t.Deactivate(); await db.SaveChangesAsync(ct); return NoContent(); }
    [HttpDelete("{slug}")] public async Task<IActionResult> Delete(string slug, CancellationToken ct) { var t = await Find(slug, ct); if (t is null) return NotFound(); t.Delete(); await db.SaveChangesAsync(ct); return NoContent(); }
    private Task<Tenant?> Find(string slug, CancellationToken ct) => db.Tenants.FirstOrDefaultAsync(x => x.TenantId == TenantId.Create(slug) && !x.IsDeleted, ct);
}
