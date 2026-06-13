using System.Security.Claims;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using AuthService.Services;
using Shared.Contracts.Auth;
using Shared.Contracts.Events;
using Shared.Contracts.Tenant;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

namespace AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext    _db;
    private readonly IJwtTokenService _jwt;
    private readonly IConfiguration   _cfg;
    private readonly IPublishEndpoint _bus;

    public AuthController(AuthDbContext db, IJwtTokenService jwt, IConfiguration cfg, IPublishEndpoint bus)
    {
        _db  = db;
        _jwt = jwt;
        _cfg = cfg;
        _bus = bus;
    }

    // ── POST /api/auth/register ────────────────────────────────────
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Slug == req.TenantId && t.IsActive, ct);
        if (tenant is null)
            return BadRequest($"Tenant '{req.TenantId}' does not exist or is inactive.");

        if (await _db.Users.AnyAsync(u => u.TenantId == req.TenantId && u.Username == req.Username, ct))
            return Conflict("Username already exists in this tenant.");

        var callerRole = User.FindFirstValue(ClaimTypes.Role);
        if (req.Role == "SuperUser" && callerRole != "SuperUser")
            return Forbid();

        var user = new AppUser
        {
            TenantId     = req.TenantId,
            Username     = req.Username,
            Email        = req.Email,
            FirstName    = req.FirstName,
            LastName     = req.LastName,
            PasswordHash = BC.HashPassword(req.Password),
            Role         = req.Role,
            IntegrationStatus = UserIntegrationStatus.Pending
        };
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        await _bus.Publish(new UserIntegrationEvent(
            NewId.NextGuid(),
            UserIntegrationOperation.Created,
            user.Id,
            user.TenantId,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.IsActive), ct);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return Ok(new { user.Id, user.Username, user.TenantId });
    }

    // ── POST /api/auth/login ───────────────────────────────────────
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var user = await _db.Users
            .Include(u => u.Claims)
            .FirstOrDefaultAsync(u => u.TenantId == req.TenantId && u.Username == req.Username, ct);

        if (user is null || !BC.Verify(req.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        if (!user.IsActive) return Forbid();

        return Ok(await IssueTokensAsync(user, ct));
    }

    // ── POST /api/auth/refresh ─────────────────────────────────────
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
    {
        var rt = await _db.RefreshTokens
            .Include(r => r.User).ThenInclude(u => u.Claims)
            .FirstOrDefaultAsync(r => r.Token == req.RefreshToken, ct);

        if (rt is null || rt.IsRevoked || rt.ExpiresAt < DateTime.UtcNow)
            return Unauthorized("Invalid or expired refresh token.");

        rt.IsRevoked = true;
        await _db.SaveChangesAsync(ct);
        return Ok(await IssueTokensAsync(rt.User, ct));
    }

    // ── POST /api/auth/revoke ──────────────────────────────────────
    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke([FromBody] RefreshRequest req, CancellationToken ct)
    {
        var rt = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == req.RefreshToken, ct);
        if (rt is null) return NotFound();
        rt.IsRevoked = true;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── GET /api/auth/me ───────────────────────────────────────────
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
        => Ok(User.Claims.Select(c => new { c.Type, c.Value }));

    // ── POST /api/auth/switch-tenant  (SuperUser only) ─────────────
    [HttpPost("switch-tenant")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> SwitchTenant([FromBody] SwitchTenantRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        var user = await _db.Users
            .Include(u => u.Claims)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null || !user.IsActive) return Unauthorized();

        var targetTenant = await _db.Tenants
            .FirstOrDefaultAsync(t => t.Slug == req.TargetTenantSlug && t.IsActive && !t.IsDeleted, ct);

        if (targetTenant is null)
            return NotFound($"Tenant '{req.TargetTenantSlug}' not found or inactive.");

        var accessToken  = _jwt.GenerateAccessToken(user, user.Claims,
                               activeTenantOverride: targetTenant.Slug);
        var refreshToken = _jwt.GenerateRefreshToken();
        var refreshDays  = int.Parse(_cfg["Jwt:RefreshTokenDays"] ?? "7");

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId    = user.Id,
            Token     = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays)
        });
        await _db.SaveChangesAsync(ct);

        var accessMinutes = int.Parse(_cfg["Jwt:AccessTokenMinutes"] ?? "60");
        var targetInstance = await _db.ServiceInstances
            .FirstOrDefaultAsync(s => s.Id == targetTenant.ServiceInstanceId && s.IsActive, ct);
        if (targetInstance is null)
            return StatusCode(503, $"Tenant '{targetTenant.Slug}' has no active service instance.");

        return Ok(new SwitchTenantResponse(
            AccessToken:      accessToken,
            RefreshToken:     refreshToken,
            ExpiresAt:        DateTime.UtcNow.AddMinutes(accessMinutes),
            ActiveTenantSlug: targetTenant.Slug,
            ActiveTenantName: targetTenant.DisplayName,
            ApiUrl:           targetInstance.ApiUrl));
    }

    // ── Private ────────────────────────────────────────────────────
    private async Task<AuthResponse> IssueTokensAsync(AppUser user, CancellationToken ct,
                                                       string? tenantOverride = null)
    {
        var accessToken  = _jwt.GenerateAccessToken(user, user.Claims, tenantOverride);
        var refreshToken = _jwt.GenerateRefreshToken();
        var refreshDays  = int.Parse(_cfg["Jwt:RefreshTokenDays"] ?? "7");

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId    = user.Id,
            Token     = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays)
        });
        await _db.SaveChangesAsync(ct);

        var activeTenantSlug = tenantOverride ?? user.TenantId;

        // Look up the tenant → service instance → API URL
        var tenant = await _db.Tenants
            .Include(t => t.ServiceInstance)
            .FirstOrDefaultAsync(t => t.Slug == activeTenantSlug, ct);

        var instance = tenant?.ServiceInstance;
        if (instance is null || !instance.IsActive)
            throw new InvalidOperationException(
                $"Tenant '{activeTenantSlug}' is not assigned to an active service instance.");

        var accessMinutes = int.Parse(_cfg["Jwt:AccessTokenMinutes"] ?? "60");
        return new AuthResponse(
            AccessToken:       accessToken,
            RefreshToken:      refreshToken,
            ExpiresAt:         DateTime.UtcNow.AddMinutes(accessMinutes),
            UserId:            user.Id,
            Username:          user.Username,
            Role:              user.Role,
            TenantId:          activeTenantSlug,
            ServiceInstanceId: instance.Id,
            ApiUrl:            instance.ApiUrl);
    }
}
