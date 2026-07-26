namespace ModernCRM.Crm.Application.DTO;

public sealed record TicketDto(int Id, string TenantId, string Subject, string? Description, int AccountId, int? ContactId, int? AssignedToAuthUserId, string Status, string Priority, DateTime? DueDate);
