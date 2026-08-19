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

public sealed class GetUserByIdHandler : IQueryHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IAuthUserRepository _users;
    public GetUserByIdHandler(IAuthUserRepository users) => _users = users;

    public async Task<UserDto?> Handle(GetUserByIdQuery query, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(TenantId.Create(query.TenantId), query.Id, ct);
        return user is null ? null : new UserDto(user.Id, user.TenantId.Value, user.Username.Value, user.FirstName, user.LastName, user.Email.Value, user.Role.ToString(), user.IsActive, user.IntegrationStatus, user.IntegrationError);
    }
}
