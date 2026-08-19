using ModernCRM.Auth.Domain.ValueObjects;
using ModernCRM.SharedKernel.ValueObjects;
using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Auth.Domain.Users;

public interface IAuthUserRepository
{
    Task<AuthUser?> GetByIdAsync(TenantId tenantId, int id, CancellationToken ct);
    Task<AuthUser?> GetByUsernameAsync(TenantId tenantId, Username username, CancellationToken ct);
    Task<bool> ExistsByEmailAsync(TenantId tenantId, string email, CancellationToken ct);
    Task<IReadOnlyList<AuthUser>> ListByTenantAsync(TenantId tenantId, string? search, int page, int pageSize, CancellationToken ct);
    Task AddAsync(AuthUser user, CancellationToken ct);
    IUnitOfWork UnitOfWork { get; }
}
