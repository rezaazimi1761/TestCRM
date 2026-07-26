namespace ModernCRM.Auth.Api.Controllers;

public sealed record ServiceInstancePayload(string? Name, string? ApiUrl, string? Description, bool? IsActive);
