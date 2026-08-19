using ModernCRM.Auth.Domain.Tenants;
using ModernCRM.Auth.Domain.Users;
using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Auth.Application.Integration;

public interface IAuthPersistenceRepository
{
    IUnitOfWork DomainUnitOfWork { get; }
    IUnitOfWork IntegrationUnitOfWork { get; }
    Task<SyncedAuthUser?> FindSyncedUserAsync(string tenantId, string username, CancellationToken cancellationToken);
    Task<SyncedAuthUser?> FindSyncedUserAsync(int crmUserId, string tenantId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<AuthUser> Items, int Total)> PageUsersAsync(string tenantId, int page, int pageSize, string? sortBy, bool sortDesc, string? search, string? role, CancellationToken cancellationToken);
    Task<AuthUser?> FindUserAsync(string tenantId, int id, CancellationToken cancellationToken);
    Task<List<Tenant>> ListTenantsAsync(CancellationToken cancellationToken);
    Task<List<AuthUser>> ListActiveUsersAsync(CancellationToken cancellationToken);
    Task<Tenant?> FindTenantAsync(string slug, bool tracking, CancellationToken cancellationToken);
    Task<bool> TenantExistsAsync(string slug, CancellationToken cancellationToken);
    Task<List<ServiceInstanceModel>> ListServiceInstancesAsync(CancellationToken cancellationToken);
    Task<ServiceInstanceModel?> FindServiceInstanceAsync(Guid id, bool tracking, CancellationToken cancellationToken);
    Task<bool> ActiveServiceInstanceExistsAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ServiceInstanceHasTenantsAsync(Guid id, CancellationToken cancellationToken);
    void Add(Tenant tenant);
    void Add(SyncedAuthUser user);
    void Add(ServiceInstanceModel instance);
    void Remove(ServiceInstanceModel instance);
}
