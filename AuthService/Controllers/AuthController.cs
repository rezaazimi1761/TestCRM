using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using AuthService.Models;
using AuthService.Services;
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
    private readonly IConfiguration  _cfg;

    public AuthController(AuthDbContext db, IJwtTokenService jwt, IConfiguration cfg)
    {
        _db  = db;
        _jwt = jwt;
        _cfg = cfg;
    }

    // ── POST /api/auth/register ────────────────────────────────────
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        if (await _db.Users.AnyAsync(u => u.TenantId == req.TenantId && u.Username == req.Username, ct))
            return Conflict("Username already exists in this tenant.");

        var user = new AppUser
        {
            TenantId     = req.TenantId,
            Username     = req.Username,
            Email        = req.Email,
            FirstName    = req.FirstName,
            LastName     = req.LastName,
            PasswordHash = BC.HashPassword(req.Password),
            Role         = req.Role
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
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

        if (!user.IsActive)
            return Forbid();

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
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(claims);
    }

    // ── Private ────────────────────────────────────────────────────
    private async Task<AuthResponse> IssueTokensAsync(AppUser user, CancellationToken ct)
    {
        var accessToken  = _jwt.GenerateAccessToken(user, user.Claims);
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
        return new AuthResponse(
            AccessToken:  accessToken,
            RefreshToken: refreshToken,
            ExpiresAt:    DateTime.UtcNow.AddMinutes(accessMinutes),
            UserId:       user.Id,
            Username:     user.Username,
            Role:         user.Role,
            TenantId:     user.TenantId);
    }
}
