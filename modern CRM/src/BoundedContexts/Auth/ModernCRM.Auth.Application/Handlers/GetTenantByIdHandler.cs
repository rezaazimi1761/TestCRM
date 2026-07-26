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
