using ModernCRM.Auth.Application.DTO;
using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Auth.Application.Queries;

public sealed record GetUserByIdQuery(string TenantId, int Id) : IQuery<UserDto?>;
