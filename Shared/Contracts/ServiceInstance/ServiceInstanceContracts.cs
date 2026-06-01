namespace Shared.Contracts.ServiceInstance;

// ── Requests ──────────────────────────────────────────────────
/// <summary>
/// Sent by a CRM API service the first time it boots, to self-register.
/// </summary>
public record RegisterServiceInstanceRequest(
    Guid    Id,
    string  Name,
    string  ApiUrl,
    string? Description);

public record UpdateServiceInstanceRequest(
    string  Name,
    string  ApiUrl,
    string? Description,
    bool    IsActive);

// ── Responses ─────────────────────────────────────────────────
public record ServiceInstanceDto(
    Guid     Id,
    string   Name,
    string   ApiUrl,
    string?  Description,
    bool     IsActive,
    DateTime CreatedAt,
    int      TenantCount);
