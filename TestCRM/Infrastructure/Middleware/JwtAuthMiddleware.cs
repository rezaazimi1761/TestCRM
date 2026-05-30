using System.Security.Claims;
using TestCRM.Infrastructure.GrpcClients;

namespace TestCRM.Infrastructure.Middleware;

/// <summary>
/// Validates the Bearer token by calling AuthService over gRPC.
/// On success it injects a ClaimsPrincipal so standard [Authorize] keeps working.
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
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, resp.UserId.ToString()),
                        new Claim(ClaimTypes.Name,           resp.Username),
                        new Claim(ClaimTypes.Role,           resp.Role),
                        new Claim("tenant_id",               resp.TenantId)
                    };
                    ctx.User = new ClaimsPrincipal(
                        new ClaimsIdentity(claims, "GrpcJwt"));
                }
            }
            catch
            {
                // AuthService unreachable – let request continue unauthenticated
            }
        }
        await _next(ctx);
    }
}
