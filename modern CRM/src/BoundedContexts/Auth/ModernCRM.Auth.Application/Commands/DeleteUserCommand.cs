using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Auth.Application.Commands;

public sealed record DeleteUserCommand(string TenantId, int Id) : ICommand<bool>;
