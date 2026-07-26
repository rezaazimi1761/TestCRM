using ModernCRM.Auth.Application.Commands;
using ModernCRM.Auth.Application.DTO;
using ModernCRM.Auth.Application.Queries;
using ModernCRM.Auth.Domain.Roles;
using ModernCRM.Auth.Domain.Tenants;
using ModernCRM.Auth.Domain.Users;
using ModernCRM.Auth.Domain.ValueObjects;
using ModernCRM.SharedKernel.Application;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Auth.Application.Handlers;

public sealed class UpdateUserHandler : ICommandHandler<UpdateUserCommand, bool>
{
    private readonly IAuthUserRepository _users;
    public UpdateUserHandler(IAuthUserRepository users) => _users = users;

    public async Task<bool> Handle(UpdateUserCommand command, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(command.Id, ct);
        if (user is null) return false;

        user.ChangeName(command.FirstName, command.LastName);
        user.ChangeEmail(Email.Create(command.Email));
        user.ChangeRole(Enum.Parse<Role>(command.Role, true), command.ActorIsSuperUser);
        if (command.IsActive) user.Activate(); else user.Deactivate();

        await _users.SaveChangesAsync(ct);
        return true;
    }
}
