using MassTransit;
using ModernCRM.Crm.Application.Users;
using ModernCRM.Crm.Application.Frontend;
using ModernCRM.SharedKernel.IntegrationEvents;

namespace ModernCRM.Crm.Infrastructure.Integration;

public sealed class CrmUserService(ICrmFrontendRepository repository, IPublishEndpoint publisher) : ICrmUserService
{
    public async Task<CrmUserPage> GetPageAsync(string tenantId, int page, int pageSize, string? sortBy, bool sortDesc, string? search, string? role, CancellationToken cancellationToken)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize < 1 ? 20 : pageSize, 1, 500);
        var result = await repository.PageUsersAsync(tenantId, page, pageSize, sortBy, sortDesc, search, role, cancellationToken);
        return new CrmUserPage(result.Items.Select(Map).ToList(), result.Total, page, pageSize);
    }

    public async Task<CrmUserDto?> GetAsync(string tenantId, int id, CancellationToken cancellationToken)
        => await repository.FindUserAsync(tenantId, id, false, cancellationToken) is { } user ? Map(user) : null;

    public async Task<(bool Created, int Id)> CreateAsync(string tenantId, CreateCrmUser request, CancellationToken cancellationToken)
    {
        if (await repository.UserIdentityExistsAsync(tenantId, request.Username, request.Email, cancellationToken)) return (false, 0);
        var user = new CrmUser { TenantId = tenantId, Username = request.Username, Email = request.Email, FirstName = request.FirstName, LastName = request.LastName, Role = request.Role, IsActive = true, SyncStatus = "Pending", CreatedAt = DateTime.UtcNow };
        await repository.UnitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            repository.Add(user);
            await repository.UnitOfWork.SaveChangesAsync(ct);
            await publisher.Publish(new CrmUserSyncRequested(Guid.NewGuid(), UserIntegrationOperation.Created, user.Id, null, tenantId, user.Username, user.Email, user.FirstName, user.LastName, user.Role, user.IsActive, request.Password), ct);
            await repository.UnitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);
        return (true, user.Id);
    }

    public async Task<bool> UpdateAsync(string tenantId, int id, UpdateCrmUser request, CancellationToken cancellationToken)
    {
        var user = await repository.FindUserAsync(tenantId, id, true, cancellationToken);
        if (user is null) return false;
        user.FirstName = request.FirstName ?? user.FirstName; user.LastName = request.LastName ?? user.LastName; user.Email = request.Email ?? user.Email; user.Role = request.Role ?? user.Role; user.IsActive = request.IsActive; user.SyncStatus = "Pending"; user.SyncError = null; user.UpdatedAt = DateTime.UtcNow;
        await PublishAndCommitAsync(user, UserIntegrationOperation.Updated, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(string tenantId, int id, CancellationToken cancellationToken)
    {
        var user = await repository.FindUserAsync(tenantId, id, true, cancellationToken);
        if (user is null) return false;
        user.IsDeleted = true; user.IsActive = false; user.SyncStatus = "Pending"; user.SyncError = null; user.UpdatedAt = DateTime.UtcNow;
        await PublishAndCommitAsync(user, UserIntegrationOperation.Deleted, cancellationToken);
        return true;
    }

    private Task PublishAndCommitAsync(CrmUser user, UserIntegrationOperation operation, CancellationToken cancellationToken)
        => repository.UnitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await publisher.Publish(new CrmUserSyncRequested(Guid.NewGuid(), operation, user.Id, user.AuthUserId, user.TenantId, user.Username, user.Email, user.FirstName, user.LastName, user.Role, user.IsActive, null), ct);
            await repository.UnitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

    private static CrmUserDto Map(CrmUser user) => new(user.Id, user.AuthUserId, user.TenantId, user.Username, user.FirstName, user.LastName, user.Email, user.Role, user.IsActive, user.IsDeleted, user.SyncStatus, user.SyncError, user.CreatedAt);
}
