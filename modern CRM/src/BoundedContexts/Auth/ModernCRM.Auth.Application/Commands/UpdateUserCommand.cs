using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Auth.Application.Commands;

public sealed record UpdateUserCommand(int Id, string Email, string FirstName, string LastName, string Role, bool IsActive, bool ActorIsSuperUser) : ICommand<bool>;
