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

public interface ITenantRepository
{
    Task AddAsync(Tenant tenant, CancellationToken ct);
    Task<Tenant?> GetByIdAsync(int id, CancellationToken ct);
    Task<Tenant?> GetByTenantIdAsync(TenantId tenantId, CancellationToken ct);
    Task<IReadOnlyList<Tenant>> ListAsync(CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

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
            PasswordHash.FromHash(_hasher.Hash(command.Password)),
            Enum.Parse<Role>(command.Role, true));

        await _users.AddAsync(user, ct);
        await _users.SaveChangesAsync(ct);
        return user.Id;
    }
}

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

public sealed class GetUsersHandler : IQueryHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    private readonly IAuthUserRepository _users;
    public GetUsersHandler(IAuthUserRepository users) => _users = users;

    public async Task<IReadOnlyList<UserDto>> Handle(GetUsersQuery query, CancellationToken ct)
    {
        var items = await _users.ListByTenantAsync(TenantId.Create(query.TenantId), query.Search, query.Page, query.PageSize, ct);
        return items.Select(u => new UserDto(u.Id, u.TenantId.Value, u.Username.Value, u.FirstName, u.LastName, u.Email.Value, u.Role.ToString(), u.IsActive, u.IntegrationStatus, u.IntegrationError)).ToList();
    }
}

public sealed class GetUserByIdHandler : IQueryHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IAuthUserRepository _users;
    public GetUserByIdHandler(IAuthUserRepository users) => _users = users;

    public async Task<UserDto?> Handle(GetUserByIdQuery query, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(query.Id, ct);
        return user is null ? null : new UserDto(user.Id, user.TenantId.Value, user.Username.Value, user.FirstName, user.LastName, user.Email.Value, user.Role.ToString(), user.IsActive, user.IntegrationStatus, user.IntegrationError);
    }
}

public sealed class CreateTenantHandler : ICommandHandler<CreateTenantCommand, int>
{
    private readonly ITenantRepository _tenants;
    public CreateTenantHandler(ITenantRepository tenants) => _tenants = tenants;

    public async Task<int> Handle(CreateTenantCommand command, CancellationToken ct)
    {
        var tenant = Tenant.Create(TenantId.Create(command.TenantId), command.DisplayName, command.ServiceInstanceId);
        await _tenants.AddAsync(tenant, ct);
        await _tenants.SaveChangesAsync(ct);
        return tenant.Id;
    }
}

public sealed class UpdateTenantHandler : ICommandHandler<UpdateTenantCommand, bool>
{
    private readonly ITenantRepository _tenants;
    public UpdateTenantHandler(ITenantRepository tenants) => _tenants = tenants;

    public async Task<bool> Handle(UpdateTenantCommand command, CancellationToken ct)
    {
        var tenant = await _tenants.GetByIdAsync(command.Id, ct);
        if (tenant is null) return false;
        tenant.Rename(command.DisplayName);
        tenant.MoveToServiceInstance(command.ServiceInstanceId);
        if (command.IsActive) tenant.Activate(); else tenant.Deactivate();
        await _tenants.SaveChangesAsync(ct);
        return true;
    }
}

public sealed class GetTenantsHandler : IQueryHandler<GetTenantsQuery, IReadOnlyList<TenantDto>>
{
    private readonly ITenantRepository _tenants;
    public GetTenantsHandler(ITenantRepository tenants) => _tenants = tenants;

    public async Task<IReadOnlyList<TenantDto>> Handle(GetTenantsQuery query, CancellationToken ct)
    {
        var items = await _tenants.ListAsync(ct);
        return items.Select(t => new TenantDto(t.Id, t.TenantId.Value, t.DisplayName, t.ServiceInstanceId, t.IsActive)).ToList();
    }
}

public sealed class GetTenantByIdHandler : IQueryHandler<GetTenantByIdQuery, TenantDto?>
{
    private readonly ITenantRepository _tenants;
    public GetTenantByIdHandler(ITenantRepository tenants) => _tenants = tenants;

    public async Task<TenantDto?> Handle(GetTenantByIdQuery query, CancellationToken ct)
    {
        var tenant = await _tenants.GetByIdAsync(query.Id, ct);
        return tenant is null ? null : new TenantDto(tenant.Id, tenant.TenantId.Value, tenant.DisplayName, tenant.ServiceInstanceId, tenant.IsActive);
    }
}
