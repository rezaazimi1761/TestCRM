using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Commands;

public sealed record DeleteAccountCommand(string TenantId, int Id) : ICommand<bool>;
