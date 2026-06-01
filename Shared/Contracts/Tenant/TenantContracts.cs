namespace Shared.Contracts.Tenant;

// ── Requests ───────────────────────────────────────────────────
public record CreateTenantRequest(
    string  Slug,
    string  DisplayName,
    Guid    ServiceInstanceId,
    string? Description);

public record UpdateTenantRequest(
    string  DisplayName,
    string? Description,
    Guid    ServiceInstanceId,
    bool    IsActive);

public record SwitchTenantRequest(string TargetTenantSlug);

// ── Responses ─────────────────────────────────────────────────
public record TenantDto(
    int      Id,
    string   Slug,
    string   DisplayName,
    string?  Description,
    bool     IsActive,
    DateTime CreatedAt,
    int      UserCount,
    Guid     ServiceInstanceId,
    string?  ServiceInstanceName,
    string?  ServiceInstanceApiUrl);

public record SwitchTenantResponse(
    string   AccessToken,
    string   RefreshToken,
    DateTime ExpiresAt,
    string   ActiveTenantSlug,
    string   ActiveTenantName,
    string   ApiUrl);
