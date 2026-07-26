using ModernCRM.SharedKernel.BuildingBlocks;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Auth.Domain.Tenants;

public sealed class Tenant : AggregateRoot<int>
{
    public TenantId TenantId { get; private set; } = null!;
    public string DisplayName { get; private set; } = string.Empty;
    public Guid ServiceInstanceId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsDeleted { get; private set; }

    private Tenant() { }

    public static Tenant Create(TenantId tenantId, string displayName, Guid serviceInstanceId)
    {
        Guard.Against(serviceInstanceId == Guid.Empty, "Tenant must be assigned to a service instance.");
        return new Tenant { TenantId = tenantId, DisplayName = Guard.NotBlank(displayName, nameof(DisplayName), 200), ServiceInstanceId = serviceInstanceId, IsActive = true };
    }

    public void Rename(string displayName) { EnsureNotDeleted(); DisplayName = Guard.NotBlank(displayName, nameof(DisplayName), 200); Touch(); }
    public void MoveToServiceInstance(Guid serviceInstanceId) { EnsureNotDeleted(); Guard.Against(serviceInstanceId == Guid.Empty, "Service instance id is required."); ServiceInstanceId = serviceInstanceId; Touch(); }
    public void Activate() { EnsureNotDeleted(); IsActive = true; Touch(); }
    public void Deactivate() { EnsureNotDeleted(); IsActive = false; Touch(); }
    public void Delete() { if (IsDeleted) return; IsActive = false; IsDeleted = true; Touch(); }
    private void EnsureNotDeleted() => Guard.Against(IsDeleted, "Deleted tenant cannot be changed.");
}
