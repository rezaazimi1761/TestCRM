using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Frontend;

public interface ICrmFrontendRepository
{
    IUnitOfWork UnitOfWork { get; }
    Task<PagedResult<LeadModel>> PageLeadsAsync(string tenantId, int page, int pageSize, string? sortBy, bool sortDesc, string? search, string? status, CancellationToken cancellationToken);
    Task<PagedResult<ActivityModel>> PageActivitiesAsync(string tenantId, int page, int pageSize, string? sortBy, bool sortDesc, string? search, string? type, bool? isCompleted, CancellationToken cancellationToken);
    Task<LeadModel?> FindLeadAsync(string tenantId, int id, CancellationToken cancellationToken);
    Task<ActivityModel?> FindActivityAsync(string tenantId, int id, CancellationToken cancellationToken);
    Task<CrmUser?> FindUserAsync(string tenantId, int id, bool tracking, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<CrmUser> Items, int Total)> PageUsersAsync(string tenantId, int page, int pageSize, string? sortBy, bool sortDesc, string? search, string? role, CancellationToken cancellationToken);
    Task<bool> UserIdentityExistsAsync(string tenantId, string username, string email, CancellationToken cancellationToken);
    Task<CrmUser?> FindUserForSyncAsync(int crmUserId, CancellationToken cancellationToken);
    Task<string?> GetContactNameAsync(string tenantId, int? contactId, CancellationToken cancellationToken);
    void Add(CrmUser entity);
    void Add(LeadModel entity);
    void Add(ActivityModel entity);
}
