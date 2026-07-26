using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Commands;

public sealed record DeleteAccountCommand(int Id) : ICommand<bool>;
