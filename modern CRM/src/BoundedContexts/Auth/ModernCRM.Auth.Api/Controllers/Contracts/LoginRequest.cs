namespace ModernCRM.Auth.Api.Controllers;

public sealed record LoginRequest(string TenantId, string Username, string Password);
