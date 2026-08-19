namespace ModernCRM.Crm.Application.Frontend;

public sealed record LeadInput(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? Company,
    string? Status,
    string? Source,
    string? Notes);

public interface ILeadService
{
    Task<PagedResult<LeadModel>> GetPageAsync(string tenantId, int page, int pageSize, string? sortBy, bool sortDesc, string? search, string? status, CancellationToken cancellationToken);
    Task<LeadModel?> GetAsync(string tenantId, int id, CancellationToken cancellationToken);
    Task<int> CreateAsync(string tenantId, LeadInput input, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(string tenantId, int id, LeadInput input, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string tenantId, int id, CancellationToken cancellationToken);
}

