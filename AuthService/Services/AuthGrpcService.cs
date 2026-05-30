using System.Security.Claims;
using AuthService.Infrastructure.Persistence;
using AuthService.Protos;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services;

/// <summary>
/// gRPC service – consumed by the CRM main service to validate tokens and look up users.
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

        var userId   = int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? principal.FindFirstValue("sub"), out var id) ? id : 0;
        var username = principal.FindFirstValue(ClaimTypes.Name) ?? "";
        var role     = principal.FindFirstValue(ClaimTypes.Role) ?? "";
        var tenant   = principal.FindFirstValue("tenant_id")     ?? "";

        return Task.FromResult(new ValidateTokenResponse
        {
            IsValid  = true,
            UserId   = userId,
            Username = username,
            Role     = role,
            TenantId = tenant
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
}
