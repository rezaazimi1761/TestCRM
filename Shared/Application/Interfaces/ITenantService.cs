namespace Shared.Application.Interfaces;

/// <summary>
/// Resolves the active tenant for the current HTTP request.
/// Reads (in priority order): JWT claim tenant_id → X-Tenant-Id header → "default".
/// </summary>
public interface ITenantService
{
    string GetCurrentTenant();
}
