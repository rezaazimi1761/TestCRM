namespace ModernCRM.Crm.Application.Users;

public sealed record CrmUserDto(int Id, int? AuthUserId, string TenantId, string Username, string FirstName, string LastName, string Email, string Role, bool IsActive, bool IsDeleted, string SyncStatus, string? SyncError, DateTime CreatedAt);
public sealed record CrmUserPage(IReadOnlyList<CrmUserDto> Items, int TotalCount, int Page, int PageSize);
public sealed record CreateCrmUser(string Username, string Email, string FirstName, string LastName, string Password, string Role);
public sealed record UpdateCrmUser(string? Email, string? FirstName, string? LastName, string? Role, bool IsActive);

public interface ICrmUserService
{
    Task<CrmUserPage> GetPageAsync(string tenantId, int page, int pageSize, string? sortBy, bool sortDesc, string? search, string? role, CancellationToken cancellationToken);
    Task<CrmUserDto?> GetAsync(string tenantId, int id, CancellationToken cancellationToken);
    Task<(bool Created, int Id)> CreateAsync(string tenantId, CreateCrmUser request, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(string tenantId, int id, UpdateCrmUser request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string tenantId, int id, CancellationToken cancellationToken);
}
