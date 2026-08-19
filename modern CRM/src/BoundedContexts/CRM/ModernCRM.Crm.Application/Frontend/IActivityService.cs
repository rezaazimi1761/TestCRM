namespace ModernCRM.Crm.Application.Frontend;

public sealed record ActivityInput(
    string? Subject,
    string? Type,
    string? Description,
    DateTime? DueDate,
    bool? IsCompleted,
    int? ContactId);

public interface IActivityService
{
    Task<PagedResult<ActivityModel>> GetPageAsync(string tenantId, int page, int pageSize, string? sortBy, bool sortDesc, string? search, string? type, bool? isCompleted, CancellationToken cancellationToken);
    Task<ActivityModel?> GetAsync(string tenantId, int id, CancellationToken cancellationToken);
    Task<int> CreateAsync(string tenantId, ActivityInput input, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(string tenantId, int id, ActivityInput input, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string tenantId, int id, CancellationToken cancellationToken);
}

