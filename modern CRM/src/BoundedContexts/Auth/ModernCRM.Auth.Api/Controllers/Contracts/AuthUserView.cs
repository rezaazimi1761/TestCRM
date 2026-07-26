namespace ModernCRM.Auth.Api.Controllers;

public sealed record AuthUserView(int Id, string TenantId, string Username, string FirstName, string LastName, string Email, string Role, bool IsActive, bool IsDeleted, string IntegrationStatus, string? IntegrationError, DateTime CreatedAt);
