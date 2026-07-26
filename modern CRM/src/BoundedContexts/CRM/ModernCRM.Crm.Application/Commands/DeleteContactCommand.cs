using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Commands;

public sealed record DeleteContactCommand(int Id) : ICommand<bool>;
