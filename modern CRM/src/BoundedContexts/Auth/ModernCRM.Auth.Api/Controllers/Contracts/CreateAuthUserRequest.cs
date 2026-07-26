namespace ModernCRM.Auth.Api.Controllers;

public sealed record CreateAuthUserRequest(string? TenantId, string? Username, string? Email, string? FirstName, string? LastName, string? Password, string? Role);
