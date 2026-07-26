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
