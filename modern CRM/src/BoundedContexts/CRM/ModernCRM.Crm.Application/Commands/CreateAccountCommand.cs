using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Commands;

public sealed record CreateAccountCommand(string TenantId, string Name, string? Industry, string? Website, string? Phone, string? Address) : ICommand<int>;
