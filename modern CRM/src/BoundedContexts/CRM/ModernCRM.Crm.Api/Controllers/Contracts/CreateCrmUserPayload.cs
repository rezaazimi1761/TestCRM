namespace ModernCRM.Crm.Api.Controllers;

public sealed record CreateCrmUserPayload(string? Username, string? Email, string? FirstName, string? LastName, string? Password, string? Role);
