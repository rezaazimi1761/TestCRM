using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Commands;

public sealed record DeleteContactCommand(string TenantId, int Id) : ICommand<bool>;
