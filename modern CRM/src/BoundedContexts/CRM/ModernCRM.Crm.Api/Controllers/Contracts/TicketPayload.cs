namespace ModernCRM.Crm.Api.Controllers;

public sealed record TicketPayload(string? Subject, string? Description, string? Status, string? Priority, int? AccountId, int? ContactId, int? AssignedToUserId, DateTime? DueDate, DateTime? ResolvedAt, string? Category, string? Notes);
