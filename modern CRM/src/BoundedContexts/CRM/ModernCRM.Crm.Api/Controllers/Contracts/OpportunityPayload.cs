namespace ModernCRM.Crm.Api.Controllers;

public sealed record OpportunityPayload(string? Title, decimal Value, string? Stage, DateTime? ExpectedCloseDate, string? Notes, int? AccountId, int? ContactId, int? AssignedToUserId);
