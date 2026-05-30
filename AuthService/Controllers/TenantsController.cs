using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using Shared.Contracts.Tenant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers;

[ApiController]
[Route("api/tenants")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly AuthDbContext _db;
    public TenantsController(AuthDbContext db) => _db = db;

    // GET /api/tenants
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var isSuperUser = User.IsInRole("SuperUser");
        var query = isSuperUser
            ? _db.Tenants.IgnoreQueryFilters()
            : _db.Tenants.AsQueryable();

        var tenants = await query
            .Where(t => !t.IsDeleted)
            .Select(t => new TenantDto(
                t.Id, t.Slug, t.DisplayName, t.Description,
                t.IsActive, t.CreatedAt,
                t.Users.Count(u => !u.IsDeleted)))
            .ToListAsync(ct);

        return Ok(tenants);
    }

    // GET /api/tenants/{slug}
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var isSuperUser = User.IsInRole("SuperUser");
        var query = isSuperUser
            ? _db.Tenants.IgnoreQueryFilters()
            : _db.Tenants.AsQueryable();

        var tenant = await query
            .Where(t => t.Slug == slug && !t.IsDeleted)
            .Select(t => new TenantDto(
                t.Id, t.Slug, t.DisplayName, t.Description,
                t.IsActive, t.CreatedAt,
                t.Users.Count(u => !u.IsDeleted)))
            .FirstOrDefaultAsync(ct);

        return tenant is null ? NotFound() : Ok(tenant);
    }

    // POST /api/tenants
    [HttpPost]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest req, CancellationToken ct)
    {
        if (await _db.Tenants.IgnoreQueryFilters().AnyAsync(t => t.Slug == req.Slug, ct))
            return Conflict($"Tenant slug '{req.Slug}' already exists.");

        var tenant = new Tenant
        {
            Slug        = req.Slug.ToLowerInvariant().Trim(),
            DisplayName = req.DisplayName,
            Description = req.Description,
            IsActive    = true
        };
        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetBySlug), new { slug = tenant.Slug },
            new TenantDto(tenant.Id, tenant.Slug, tenant.DisplayName,
                          tenant.Description, tenant.IsActive, tenant.CreatedAt, 0));
    }

    // PUT /api/tenants/{slug}
    [HttpPut("{slug}")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> Update(string slug, [FromBody] UpdateTenantRequest req, CancellationToken ct)
    {
        var tenant = await _db.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Slug == slug && !t.IsDeleted, ct);
        if (tenant is null) return NotFound();

        tenant.DisplayName = req.DisplayName;
        tenant.Description = req.Description;
        tenant.IsActive    = req.IsActive;
        tenant.UpdatedAt   = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // DELETE /api/tenants/{slug}
    [HttpDelete("{slug}")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> Delete(string slug, CancellationToken ct)
    {
        var tenant = await _db.Tenants.IgnoreQueryFilters()
            .Include(t => t.Users)
            .FirstOrDefaultAsync(t => t.Slug == slug && !t.IsDeleted, ct);
        if (tenant is null) return NotFound();

        tenant.IsDeleted = true;
        tenant.UpdatedAt = DateTime.UtcNow;
        foreach (var u in tenant.Users.Where(u => !u.IsDeleted)) u.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // PATCH /api/tenants/{slug}/activate
    [HttpPatch("{slug}/activate")]
    [Authorize(Roles = "SuperUser")]
    public Task<IActionResult> Activate(string slug, CancellationToken ct)
        => SetActive(slug, true, ct);

    // PATCH /api/tenants/{slug}/deactivate
    [HttpPatch("{slug}/deactivate")]
    [Authorize(Roles = "SuperUser")]
    public Task<IActionResult> Deactivate(string slug, CancellationToken ct)
        => SetActive(slug, false, ct);

    private async Task<IActionResult> SetActive(string slug, bool active, CancellationToken ct)
    {
        var tenant = await _db.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Slug == slug && !t.IsDeleted, ct);
        if (tenant is null) return NotFound();
        tenant.IsActive  = active;
        tenant.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
