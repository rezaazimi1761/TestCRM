using ModernCRM.Auth.Application.DTO;
using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Auth.Application.Queries;

public sealed record GetTenantByIdQuery(int Id) : IQuery<TenantDto?>;
