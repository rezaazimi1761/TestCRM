using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Auth.Application.Commands;

public sealed record CreateUserCommand(string TenantId, string Username, string Email, string FirstName, string LastName, string Password, string Role) : ICommand<int>;
public sealed record UpdateUserCommand(int Id, string Email, string FirstName, string LastName, string Role, bool IsActive, bool ActorIsSuperUser) : ICommand<bool>;
public sealed record DeleteUserCommand(int Id) : ICommand<bool>;
public sealed record CreateTenantCommand(string TenantId, string DisplayName, Guid ServiceInstanceId) : ICommand<int>;
public sealed record UpdateTenantCommand(int Id, string DisplayName, Guid ServiceInstanceId, bool IsActive) : ICommand<bool>;
