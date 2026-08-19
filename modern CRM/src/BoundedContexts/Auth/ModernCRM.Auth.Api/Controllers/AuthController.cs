using Microsoft.AspNetCore.Mvc;
using ModernCRM.Auth.Application.Handlers;
using ModernCRM.Auth.Domain.Roles;
using ModernCRM.Auth.Domain.Users;
using ModernCRM.Auth.Domain.ValueObjects;
using ModernCRM.Auth.Application.Integration;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Auth.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IAuthUserRepository users,
    ITenantRepository tenants,
    IPasswordHasher passwordHasher,
    IJwtTokenService tokens,
    IConfiguration configuration,
    IAuthPersistenceRepository persistence) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.TenantId) ||
            string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Tenant, username and password are required." });
        }

        var tenantId = TenantId.Create(request.TenantId);
        var tenant = await tenants.GetByTenantIdAsync(tenantId, ct);
        if (tenant is null || !tenant.IsActive)
        {
            return Unauthorized(new { message = "Invalid tenant, username, or password." });
        }

        var user = await users.GetByUsernameAsync(tenantId, Username.Create(request.Username), ct);
        if (user is null)
        {
            var persistent = await persistence.FindSyncedUserAsync(tenantId.Value, request.Username, ct);
            if (persistent is null || !passwordHasher.Verify(request.Password, persistent.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid tenant, username, or password." });
            }

            user = AuthUser.Rehydrate(
                persistent.Id,
                tenantId,
                Username.Create(persistent.Username),
                Email.Create(persistent.Email),
                persistent.FirstName,
                persistent.LastName,
                PasswordHash.FromHash(persistent.PasswordHash),
                Enum.Parse<Role>(persistent.Role, true),
                persistent.IsActive,
                persistent.CreatedAt);
        }
        else if (!user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash.Value))
        {
            return Unauthorized(new { message = "Invalid tenant, username, or password." });
        }

        return Ok(new AuthResponse(
            tokens.GenerateAccessToken(user),
            tokens.GenerateRefreshToken(),
            DateTime.UtcNow.AddMinutes(configuration.GetValue("Jwt:AccessTokenMinutes", 60)),
            user.Id,
            user.Username.Value,
            user.Role.ToString(),
            user.TenantId.Value,
            tenant.ServiceInstanceId,
            configuration["Seed:DefaultServiceInstanceUrl"] ?? "http://localhost:9040"));
    }
}
