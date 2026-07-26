namespace ModernCRM.Crm.Api.Controllers;

public sealed record UpdateCrmUserPayload(string? Email, string? FirstName, string? LastName, string? Role, bool IsActive);
