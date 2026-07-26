namespace ModernCRM.Auth.Api.Controllers;

public sealed record TenantPayload(string? Slug, string? DisplayName, Guid ServiceInstanceId, string? Description, bool? IsActive);
