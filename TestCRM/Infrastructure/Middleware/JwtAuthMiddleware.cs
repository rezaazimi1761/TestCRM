using System.Security.Claims;
using TestCRM.Infrastructure.GrpcClients;

namespace TestCRM.Infrastructure.Middleware;

/// <summary>
/// Validates the Bearer token by calling AuthService over gRPC.
/// Injects a ClaimsPrincipal with tenant_id, role, etc., so standard
/// [Authorize] and the TenantService both work transparently.
/// </summary>
public class JwtAuthMiddleware
{
    private readonly RequestDelegate _next;
    public JwtAuthMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext ctx, IAuthGrpcClient authClient)
    {
        var authHeader = ctx.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ") == true)
        {
            var token = authHeader["Bearer ".Length..].Trim();
            try
            {
                var resp = await authClient.ValidateTokenAsync(token, ctx.RequestAborted);
                if (resp.IsValid)
                {
                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.NameIdentifier, resp.UserId.ToString()),
                        new(ClaimTypes.Name,           resp.Username),
                        new(ClaimTypes.Role,           resp.Role),
                        new("tenant_id",               resp.TenantId),
                        new("home_tenant_id",          resp.HomeTenantId),
                    };

                    if (resp.TenantSwitched)
                        claims.Add(new Claim("tenant_switched", "true"));

                    ctx.User = new ClaimsPrincipal(
                        new ClaimsIdentity(claims, "GrpcJwt"));
                }
            }
            catch
            {
                // AuthService unreachable – pass unauthenticated
            }
        }
        await _next(ctx);
    }
}
