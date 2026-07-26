namespace ModernCRM.Auth.Application.DTO;

public sealed record UserDto(int Id, string TenantId, string Username, string FirstName, string LastName, string Email, string Role, bool IsActive, string IntegrationStatus, string? IntegrationError);
public sealed record TenantDto(int Id, string TenantId, string DisplayName, Guid ServiceInstanceId, bool IsActive);
