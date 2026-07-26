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
