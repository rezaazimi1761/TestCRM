using ModernCRM.Auth.Application.Commands;
using ModernCRM.Auth.Application.DTO;
using ModernCRM.Auth.Application.Queries;
using ModernCRM.Auth.Domain.Roles;
using ModernCRM.Auth.Domain.Tenants;
using ModernCRM.Auth.Domain.Users;
using ModernCRM.Auth.Domain.ValueObjects;
using ModernCRM.SharedKernel.Application;
using ModernCRM.SharedKernel.ValueObjects;
using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Auth.Application.Handlers;

public sealed class CreateUserHandler : ICommandHandler<CreateUserCommand, int>
{
    private readonly IAuthUserRepository _users;
    private readonly ITenantRepository _tenants;
    private readonly IPasswordHasher _hasher;

    public CreateUserHandler(IAuthUserRepository users, ITenantRepository tenants, IPasswordHasher hasher)
    {
        _users = users;
        _tenants = tenants;
        _hasher = hasher;
    }

    public async Task<int> Handle(CreateUserCommand command, CancellationToken ct)
    {
        var tenantId = TenantId.Create(command.TenantId);
        var username = Username.Create(command.Username);
        var tenant = await _tenants.GetByTenantIdAsync(tenantId, ct) ?? throw new InvalidOperationException("Tenant does not exist.");

        if (await _users.GetByUsernameAsync(tenantId, username, ct) is not null)
            throw new InvalidOperationException("Username already exists.");

        if (await _users.ExistsByEmailAsync(tenantId, command.Email, ct))
            throw new InvalidOperationException("Email already exists.");

        var user = AuthUser.Register(
            tenant.TenantId,
            username,
            Email.Create(command.Email),
            command.FirstName,
            command.LastName,
            PasswordHash.FromHash(_hasher.Hash(Password.Create(command.Password).Value)),
            Guard.ValidEnum<Role>(command.Role, nameof(command.Role)));

        await _users.AddAsync(user, ct);
        await _users.UnitOfWork.SaveChangesAsync(ct);
        return user.Id;
    }
}
