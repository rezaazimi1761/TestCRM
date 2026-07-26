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

public sealed class DeleteUserHandler : ICommandHandler<DeleteUserCommand, bool>
{
    private readonly IAuthUserRepository _users;
    public DeleteUserHandler(IAuthUserRepository users) => _users = users;

    public async Task<bool> Handle(DeleteUserCommand command, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(command.Id, ct);
        if (user is null) return false;
        user.LogicalDelete();
        await _users.SaveChangesAsync(ct);
        return true;
    }
}
