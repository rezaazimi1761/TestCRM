using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Commands;

public sealed record DeleteTicketCommand(string TenantId, int Id) : ICommand<bool>;
