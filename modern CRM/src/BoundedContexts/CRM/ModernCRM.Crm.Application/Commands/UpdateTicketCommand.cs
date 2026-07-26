using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Commands;

public sealed record UpdateTicketCommand(int Id, string Subject, string Priority, string Status, DateTime? DueDate, string? Description, int? ContactId, int? AssignedToAuthUserId) : ICommand<bool>;
