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
