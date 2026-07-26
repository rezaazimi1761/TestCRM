namespace ModernCRM.Crm.Api.Controllers;

public sealed record ContactPayload(string? FirstName, string? LastName, string? Email, string? Phone, string? Company, string? JobTitle, string? Notes, int? AccountId);
