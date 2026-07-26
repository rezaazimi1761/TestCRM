using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Auth.Application.Commands;

public sealed record DeleteUserCommand(int Id) : ICommand<bool>;
