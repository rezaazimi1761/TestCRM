namespace ModernCRM.Web.Services;

public sealed record LoginRequest(string TenantId, string Username, string Password);
