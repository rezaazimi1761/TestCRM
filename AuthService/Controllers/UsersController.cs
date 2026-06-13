using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Contracts.Auth;
using Shared.Contracts.Events;
using BC = BCrypt.Net.BCrypt;

namespace AuthService.Controllers;

[ApiController]
[Authorize(Roles = "Admin,SuperUser")]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private const int MaxPageSize = 500;

    private readonly AuthDbContext _db;
    private readonly IPublishEndpoint _bus;

    public UsersController(AuthDbContext db, IPublishEndpoint bus)
    {
        _db = db;
        _bus = bus;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        [FromQuery] string? tenantId = null,
        CancellationToken ct = default)
    {
        if (page < 1) return BadRequest(new { message = "'page' must be >= 1." });
        if (pageSize < 1 || pageSize > MaxPageSize)
            return BadRequest(new { message = $"'pageSize' must be between 1 and {MaxPageSize}." });

        var tenant = ResolveTenant(tenantId);
        var q = _db.Users.AsNoTracking().Where(u =>
            u.TenantId == tenant &&
            (!u.IsDeleted || u.IntegrationStatus == UserIntegrationStatus.Failed));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            q = q.Where(u =>
                u.Username.ToLower().Contains(s) ||
                u.FirstName.ToLower().Contains(s) ||
                u.LastName.ToLower().Contains(s) ||
                u.Email.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(role))
            q = q.Where(u => u.Role == role);

        var total = await q.CountAsync(ct);

        q = sortBy?.ToLowerInvariant() switch
        {
            "username" => sortDesc ? q.OrderByDescending(u => u.Username) : q.OrderBy(u => u.Username),
            "firstname" => sortDesc ? q.OrderByDescending(u => u.FirstName) : q.OrderBy(u => u.FirstName),
            "lastname" => sortDesc ? q.OrderByDescending(u => u.LastName) : q.OrderBy(u => u.LastName),
            "email" => sortDesc ? q.OrderByDescending(u => u.Email) : q.OrderBy(u => u.Email),
            "role" => sortDesc ? q.OrderByDescending(u => u.Role) : q.OrderBy(u => u.Role),
            _ => sortDesc ? q.OrderByDescending(u => u.Id) : q.OrderBy(u => u.Id)
        };

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserAdminDto(
                u.Id, u.TenantId, u.Username, u.FirstName, u.LastName,
                u.Email, u.Role, u.IsActive, u.IsDeleted,
                u.IntegrationStatus, u.IntegrationError, u.CreatedAt))
            .ToListAsync(ct);

        return Ok(new PagedResult<UserAdminDto>(items, total, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var tenant = ResolveTenant(null);
        var q = _db.Users.AsNoTracking().Where(u => u.Id == id && !u.IsDeleted);
        if (!User.IsInRole("SuperUser"))
            q = q.Where(u => u.TenantId == tenant);

        var user = await q
            .Select(u => new UserAdminDto(
                u.Id, u.TenantId, u.Username, u.FirstName, u.LastName,
                u.Email, u.Role, u.IsActive, u.IsDeleted,
                u.IntegrationStatus, u.IntegrationError, u.CreatedAt))
            .FirstOrDefaultAsync(ct);

        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var tenantId = ResolveTenant(req.TenantId);
        var validation = ValidateCreate(req, tenantId, out var username, out var email, out var role);
        if (validation is not null) return validation;

        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Slug == tenantId && t.IsActive, ct);
        if (tenant is null) return BadRequest(new { message = $"Tenant '{tenantId}' does not exist or is inactive." });

        if (await _db.Users.AnyAsync(u => u.TenantId == tenantId && u.Username == username, ct))
            return Conflict(new { message = "Username already exists in this tenant." });
        if (await _db.Users.AnyAsync(u => u.TenantId == tenantId && u.Email == email, ct))
            return Conflict(new { message = "Email already exists in this tenant." });

        var user = new AppUser
        {
            TenantId = tenantId,
            Username = username,
            Email = email,
            FirstName = req.FirstName.Trim(),
            LastName = req.LastName.Trim(),
            PasswordHash = BC.HashPassword(req.Password),
            Role = role,
            IsActive = true,
            IntegrationStatus = UserIntegrationStatus.Pending
        };

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        await PublishIntegrationAsync(user, UserIntegrationOperation.Created, ct);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user.Id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest req, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, ct);
        if (user is null) return NotFound();
        if (!User.IsInRole("SuperUser") && user.TenantId != ResolveTenant(null)) return NotFound();

        if (!new EmailAddressAttribute().IsValid(req.Email))
            return BadRequest(new { message = "Email is not valid." });

        var role = NormalizeRole(req.Role);
        if (role is "Admin" or "SuperUser" && !User.IsInRole("SuperUser"))
            return Forbid();

        var email = req.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Id != id && u.TenantId == user.TenantId && u.Email == email, ct))
            return Conflict(new { message = "Email already exists in this tenant." });

        user.FirstName = req.FirstName.Trim();
        user.LastName = req.LastName.Trim();
        user.Email = email;
        user.Role = role;
        user.IsActive = req.IsActive;
        user.IntegrationStatus = UserIntegrationStatus.Pending;
        user.IntegrationError = null;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        await PublishIntegrationAsync(user, UserIntegrationOperation.Updated, ct);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, ct);
        if (user is null) return NotFound();
        if (!User.IsInRole("SuperUser") && user.TenantId != ResolveTenant(null)) return NotFound();

        user.IsDeleted = true;
        user.IsActive = false;
        user.IntegrationStatus = UserIntegrationStatus.Pending;
        user.IntegrationError = null;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        await PublishIntegrationAsync(user, UserIntegrationOperation.Deleted, ct);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return NoContent();
    }

    private IActionResult? ValidateCreate(RegisterRequest req, string tenantId, out string username, out string email, out string role)
    {
        username = req.Username.Trim().ToLowerInvariant();
        email = req.Email.Trim().ToLowerInvariant();
        role = NormalizeRole(req.Role);

        if (string.IsNullOrWhiteSpace(tenantId)) return BadRequest(new { message = "Tenant is required." });
        if (string.IsNullOrWhiteSpace(username)) return BadRequest(new { message = "Username is required." });
        if (string.IsNullOrWhiteSpace(req.FirstName)) return BadRequest(new { message = "First name is required." });
        if (string.IsNullOrWhiteSpace(req.LastName)) return BadRequest(new { message = "Last name is required." });
        if (!new EmailAddressAttribute().IsValid(email)) return BadRequest(new { message = "Email is not valid." });
        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6)
            return BadRequest(new { message = "Password must be at least 6 characters." });
        if (role is "Admin" or "SuperUser" && !User.IsInRole("SuperUser"))
            return Forbid();

        return null;
    }

    private string ResolveTenant(string? requestedTenant)
    {
        var claimTenant = User.FindFirstValue("tenant_id")
            ?? User.FindFirstValue("home_tenant_id")
            ?? "default";

        return User.IsInRole("SuperUser") && !string.IsNullOrWhiteSpace(requestedTenant)
            ? requestedTenant.Trim().ToLowerInvariant()
            : claimTenant;
    }

    private static string NormalizeRole(string? role)
        => role is "Admin" or "SuperUser" ? role : "User";

    private Task PublishIntegrationAsync(AppUser user, UserIntegrationOperation operation, CancellationToken ct)
        => _bus.Publish(new UserIntegrationEvent(
            NewId.NextGuid(),
            operation,
            user.Id,
            user.TenantId,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.IsActive), ct);
}
