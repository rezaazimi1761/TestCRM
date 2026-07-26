using ModernCRM.Auth.Application.DTO;
using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Auth.Application.Queries;

public sealed record GetUsersQuery(string TenantId, int Page = 1, int PageSize = 20, string? Search = null) : IQuery<IReadOnlyList<UserDto>>;
public sealed record GetUserByIdQuery(int Id) : IQuery<UserDto?>;
public sealed record GetTenantsQuery() : IQuery<IReadOnlyList<TenantDto>>;
public sealed record GetTenantByIdQuery(int Id) : IQuery<TenantDto?>;
