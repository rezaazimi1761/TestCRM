namespace ModernCRM.Auth.Api.Controllers;

public sealed record UpdateAuthUserRequest(string? Email, string? FirstName, string? LastName, string? Role, bool IsActive);
