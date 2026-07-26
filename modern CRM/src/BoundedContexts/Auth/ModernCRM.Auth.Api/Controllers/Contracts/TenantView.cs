namespace ModernCRM.Auth.Api.Controllers;

public sealed record TenantView(int Id, string Slug, string DisplayName, string? Description, bool IsActive, DateTime CreatedAt, int UserCount, Guid ServiceInstanceId, string? ServiceInstanceName, string? ServiceInstanceApiUrl);
