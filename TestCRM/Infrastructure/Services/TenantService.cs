using TestCRM.Application.Interfaces;

namespace TestCRM.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentTenant()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return "default";

        // Resolve tenant from header, JWT claim, or subdomain
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerTenant))
            return headerTenant.ToString();

        var claim = context.User?.FindFirst("tenant_id");
        if (claim != null) return claim.Value;

        return "default";
    }
}
