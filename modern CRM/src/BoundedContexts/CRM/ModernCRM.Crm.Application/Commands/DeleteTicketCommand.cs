using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Commands;

public sealed record DeleteTicketCommand(int Id) : ICommand<bool>;
