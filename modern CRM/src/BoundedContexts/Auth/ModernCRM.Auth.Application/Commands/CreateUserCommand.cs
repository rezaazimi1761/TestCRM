using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Auth.Application.Commands;

public sealed record CreateUserCommand(string TenantId, string Username, string Email, string FirstName, string LastName, string Password, string Role) : ICommand<int>;
