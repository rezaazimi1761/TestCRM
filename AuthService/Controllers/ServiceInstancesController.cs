using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.ServiceInstance;

namespace AuthService.Controllers;

[ApiController]
[Route("api/service-instances")]
public class ServiceInstancesController : ControllerBase
{
    private readonly AuthDbContext _db;
    public ServiceInstancesController(AuthDbContext db) => _db = db;

    // ── Self-registration from a CRM API service on first boot ────
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterServiceInstanceRequest req, CancellationToken ct)
    {
        var existing = await _db.ServiceInstances.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == req.Id, ct);

        if (existing is null)
        {
            _db.ServiceInstances.Add(new ServiceInstance
            {
                Id          = req.Id,
                Name        = req.Name,
                ApiUrl      = req.ApiUrl,
                Description = req.Description,
                IsActive    = true
            });
        }
        else
        {
            // Heartbeat / URL refresh
            existing.Name      = req.Name;
            existing.ApiUrl    = req.ApiUrl;
            existing.IsActive  = true;
            existing.IsDeleted = false;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── CRUD (SuperUser) ───────────────────────────────────────────
    [HttpGet]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _db.ServiceInstances
            .Select(s => new ServiceInstanceDto(
                s.Id, s.Name, s.ApiUrl, s.Description, s.IsActive, s.CreatedAt,
                s.Tenants.Count(t => !t.IsDeleted)))
            .ToListAsync(ct));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var s = await _db.ServiceInstances.Include(x => x.Tenants)
                    .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (s is null) return NotFound();
        return Ok(new ServiceInstanceDto(s.Id, s.Name, s.ApiUrl, s.Description,
                  s.IsActive, s.CreatedAt, s.Tenants.Count(t => !t.IsDeleted)));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceInstanceRequest req, CancellationToken ct)
    {
        var s = await _db.ServiceInstances.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (s is null) return NotFound();
        s.Name        = req.Name;
        s.ApiUrl      = req.ApiUrl;
        s.Description = req.Description;
        s.IsActive    = req.IsActive;
        s.UpdatedAt   = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var s = await _db.ServiceInstances.Include(x => x.Tenants)
                    .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (s is null) return NotFound();
        if (s.Tenants.Any(t => !t.IsDeleted))
            return Conflict("Cannot delete a service instance that still hosts tenants.");
        s.IsDeleted = true;
        s.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
