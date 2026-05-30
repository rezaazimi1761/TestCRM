using System.Security.Claims;
using AuthService.Infrastructure.Persistence;
using Shared.Protos;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services;

/// <summary>
/// gRPC service consumed by CRM and other internal services.
/// Implements the server-side base class generated in Shared.
/// </summary>
public class AuthGrpcService : AuthGrpc.AuthGrpcBase
{
    private readonly AuthDbContext    _db;
    private readonly IJwtTokenService _jwt;

    public AuthGrpcService(AuthDbContext db, IJwtTokenService jwt)
    {
        _db  = db;
        _jwt = jwt;
    }

    // ── ValidateToken ──────────────────────────────────────────────
    public override Task<ValidateTokenResponse> ValidateToken(
        ValidateTokenRequest request, ServerCallContext context)
    {
        var principal = _jwt.ValidatePrincipal(request.Token);
        if (principal is null)
            return Task.FromResult(new ValidateTokenResponse { IsValid = false });

        var userId = int.TryParse(
            principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub"),
            out var id) ? id : 0;

        var tenantSwitched = principal.FindFirstValue("tenant_switched") == "true";

        return Task.FromResult(new ValidateTokenResponse
        {
            IsValid        = true,
            UserId         = userId,
            Username       = principal.FindFirstValue(ClaimTypes.Name)     ?? "",
            Role           = principal.FindFirstValue(ClaimTypes.Role)     ?? "",
            TenantId       = principal.FindFirstValue("tenant_id")         ?? "",
            HomeTenantId   = principal.FindFirstValue("home_tenant_id")    ?? "",
            TenantSwitched = tenantSwitched
        });
    }

    // ── GetUserById ────────────────────────────────────────────────
    public override async Task<UserResponse> GetUserById(
        GetUserByIdRequest request, ServerCallContext context)
    {
        var user = await _db.Users.FindAsync(request.Id);
        if (user is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"User {request.Id} not found"));

        return new UserResponse
        {
            Id        = user.Id,
            Username  = user.Username,
            Email     = user.Email,
            FirstName = user.FirstName,
            LastName  = user.LastName,
            Role      = user.Role,
            TenantId  = user.TenantId,
            IsActive  = user.IsActive
        };
    }

    // ── GetUserClaims ──────────────────────────────────────────────
    public override async Task<UserClaimsResponse> GetUserClaims(
        GetUserClaimsRequest request, ServerCallContext context)
    {
        var claims = await _db.UserClaims
            .Where(c => c.UserId == request.UserId)
            .Select(c => new ClaimDto { Type = c.ClaimType, Value = c.ClaimValue })
            .ToListAsync();

        var response = new UserClaimsResponse();
        response.Claims.AddRange(claims);
        return response;
    }

    // ── GetTenantBySlug ────────────────────────────────────────────
    public override async Task<TenantResponse> GetTenantBySlug(
        GetTenantBySlugRequest request, ServerCallContext context)
    {
        var tenant = await _db.Tenants
            .FirstOrDefaultAsync(t => t.Slug == request.Slug && !t.IsDeleted);

        if (tenant is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Tenant '{request.Slug}' not found"));

        return new TenantResponse
        {
            Id          = tenant.Id,
            Slug        = tenant.Slug,
            DisplayName = tenant.DisplayName,
            IsActive    = tenant.IsActive
        };
    }
}
