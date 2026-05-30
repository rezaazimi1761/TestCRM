namespace Shared.Contracts.Tenant;

// ── Requests ───────────────────────────────────────────────────
public record CreateTenantRequest(
    string  Slug,
    string  DisplayName,
    string? Description);

public record UpdateTenantRequest(
    string  DisplayName,
    string? Description,
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
    int      UserCount);

public record SwitchTenantResponse(
    string   AccessToken,
    string   RefreshToken,
    DateTime ExpiresAt,
    string   ActiveTenantSlug,
    string   ActiveTenantName);
