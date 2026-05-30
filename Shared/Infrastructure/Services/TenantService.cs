using Shared.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Shared.Infrastructure.Services;

/// <summary>
/// Concrete implementation of ITenantService.
/// Resolution order: JWT claim "tenant_id" → "X-Tenant-Id" header → "default".
/// </summary>
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _http;
    public TenantService(IHttpContextAccessor http) => _http = http;

    public string GetCurrentTenant()
    {
        var ctx = _http.HttpContext;
        if (ctx is null) return "default";

        // 1. JWT claim injected by JwtAuthMiddleware (AuthService gRPC validation)
        var claim = ctx.User?.FindFirst("tenant_id");
        if (claim is not null && !string.IsNullOrWhiteSpace(claim.Value))
            return claim.Value;

        // 2. Explicit header (useful for service-to-service calls)
        if (ctx.Request.Headers.TryGetValue("X-Tenant-Id", out var header) &&
            !string.IsNullOrWhiteSpace(header))
            return header.ToString();

        return "default";
    }
}
