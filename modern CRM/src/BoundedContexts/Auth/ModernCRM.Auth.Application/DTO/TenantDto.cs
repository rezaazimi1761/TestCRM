namespace ModernCRM.Auth.Application.DTO;

public sealed record TenantDto(int Id, string TenantId, string DisplayName, Guid ServiceInstanceId, bool IsActive);
