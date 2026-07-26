namespace ModernCRM.Crm.Api.Controllers;

public sealed record LeadPayload(string? FirstName, string? LastName, string? Email, string? Phone, string? Company, string? Status, string? Source, string? Notes);
