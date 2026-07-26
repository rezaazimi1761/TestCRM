using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Commands;

public sealed record CreateTicketCommand(string TenantId, string Subject, int AccountId, string Priority, DateTime? DueDate, string? Description, int? ContactId, int? AssignedToAuthUserId) : ICommand<int>;
