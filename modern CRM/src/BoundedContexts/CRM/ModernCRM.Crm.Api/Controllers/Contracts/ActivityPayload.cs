namespace ModernCRM.Crm.Api.Controllers;

public sealed record ActivityPayload(string? Subject, string? Type, string? Description, DateTime? DueDate, bool? IsCompleted, int? ContactId);
