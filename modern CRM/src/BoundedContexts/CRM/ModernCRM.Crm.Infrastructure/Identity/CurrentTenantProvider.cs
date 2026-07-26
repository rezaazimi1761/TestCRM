using Microsoft.AspNetCore.Http;

namespace ModernCRM.Crm.Infrastructure.Identity;

public interface ICurrentTenantProvider { string TenantId { get; } }
public sealed class CurrentTenantProvider(IHttpContextAccessor accessor) : ICurrentTenantProvider
{
    public string TenantId => accessor.HttpContext?.User.FindFirst("tenant_id")?.Value
        ?? accessor.HttpContext?.Request.Headers["X-Tenant-Id"].FirstOrDefault()
        ?? "default";
}