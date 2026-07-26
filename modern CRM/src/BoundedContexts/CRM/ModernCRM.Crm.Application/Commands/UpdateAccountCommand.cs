using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Commands;

public sealed record UpdateAccountCommand(int Id, string Name, string? Industry, string? Website, string? Phone, string? Address) : ICommand<bool>;
