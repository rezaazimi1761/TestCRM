using System.Linq.Expressions;
using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Auth.Domain.Users;

public sealed class ActiveUsersInTenantSpecification : Specification<AuthUser>
{
    private readonly string _tenantId;
    public ActiveUsersInTenantSpecification(string tenantId) => _tenantId = tenantId;
    public override Expression<Func<AuthUser, bool>> Criteria => user => user.TenantId.Value == _tenantId && user.IsActive && !user.IsDeleted;
}
