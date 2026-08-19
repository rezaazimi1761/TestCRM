using Microsoft.EntityFrameworkCore;
using ModernCRM.Crm.Infrastructure.Persistence;
using ModernCRM.SharedKernel.Application;
using ModernCRM.Crm.Application.Frontend;

namespace ModernCRM.Crm.Infrastructure.Integration;

public sealed class CrmPersistenceRepository : ICrmFrontendRepository
{
    private readonly CrmIntegrationDbContext _db;
    private readonly CrmDbContext _domainDb;

    public CrmPersistenceRepository(CrmIntegrationDbContext db, CrmDbContext domainDb)
    {
        _db = db;
        _domainDb = domainDb;
        UnitOfWork = new EfUnitOfWork(db);
    }

    public IUnitOfWork UnitOfWork { get; }

    public async Task<PagedResult<LeadModel>> PageLeadsAsync(
        string tenantId, int page, int pageSize, string? sortBy, bool sortDesc,
        string? search, string? status, CancellationToken cancellationToken)
    {
        var query = _db.Leads.Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => (x.FirstName != null && x.FirstName.Contains(search)) ||
                                     (x.LastName != null && x.LastName.Contains(search)) ||
                                     (x.Email != null && x.Email.Contains(search)) ||
                                     (x.Company != null && x.Company.Contains(search)));
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize < 1 ? 20 : pageSize, 1, 500);
        var total = await query.CountAsync(cancellationToken);
        query = sortBy?.ToLowerInvariant() switch
        {
            "firstname" => sortDesc ? query.OrderByDescending(x => x.FirstName) : query.OrderBy(x => x.FirstName),
            "lastname" => sortDesc ? query.OrderByDescending(x => x.LastName) : query.OrderBy(x => x.LastName),
            "email" => sortDesc ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
            "company" => sortDesc ? query.OrderByDescending(x => x.Company) : query.OrderBy(x => x.Company),
            "status" => sortDesc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => sortDesc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
        };
        var items = await query.AsNoTracking().Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<LeadModel>(items, total, page, pageSize);
    }

    public async Task<PagedResult<ActivityModel>> PageActivitiesAsync(
        string tenantId, int page, int pageSize, string? sortBy, bool sortDesc,
        string? search, string? type, bool? isCompleted, CancellationToken cancellationToken)
    {
        var query = _db.Activities.Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => (x.Subject != null && x.Subject.Contains(search)) ||
                                     (x.Description != null && x.Description.Contains(search)) ||
                                     (x.ContactName != null && x.ContactName.Contains(search)));
        if (!string.IsNullOrWhiteSpace(type)) query = query.Where(x => x.Type == type);
        if (isCompleted.HasValue) query = query.Where(x => x.IsCompleted == isCompleted.Value);
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize < 1 ? 20 : pageSize, 1, 500);
        var total = await query.CountAsync(cancellationToken);
        query = sortBy?.ToLowerInvariant() switch
        {
            "subject" => sortDesc ? query.OrderByDescending(x => x.Subject) : query.OrderBy(x => x.Subject),
            "type" => sortDesc ? query.OrderByDescending(x => x.Type) : query.OrderBy(x => x.Type),
            "duedate" => sortDesc ? query.OrderByDescending(x => x.DueDate) : query.OrderBy(x => x.DueDate),
            "iscompleted" => sortDesc ? query.OrderByDescending(x => x.IsCompleted) : query.OrderBy(x => x.IsCompleted),
            _ => sortDesc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
        };
        var items = await query.AsNoTracking().Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<ActivityModel>(items, total, page, pageSize);
    }

    public Task<LeadModel?> FindLeadAsync(string tenantId, int id, CancellationToken cancellationToken)
        => _db.Leads.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, cancellationToken);

    public Task<ActivityModel?> FindActivityAsync(string tenantId, int id, CancellationToken cancellationToken)
        => _db.Activities.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, cancellationToken);

    public Task<CrmUser?> FindUserAsync(string tenantId, int id, bool tracking, CancellationToken cancellationToken = default)
    {
        var query = _db.Users.Where(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);
        return (tracking ? query : query.AsNoTracking()).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<CrmUser> Items, int Total)> PageUsersAsync(
        string tenantId, int page, int pageSize, string? sortBy, bool sortDesc, string? search, string? role, CancellationToken cancellationToken)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize < 1 ? 20 : pageSize, 1, 500);
        var query = _db.Users.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Username.Contains(search) || x.FirstName.Contains(search) || x.LastName.Contains(search) || x.Email.Contains(search));
        if (!string.IsNullOrWhiteSpace(role)) query = query.Where(x => x.Role == role);
        query = sortBy?.ToLowerInvariant() switch
        {
            "username" => sortDesc ? query.OrderByDescending(x => x.Username) : query.OrderBy(x => x.Username),
            "firstname" => sortDesc ? query.OrderByDescending(x => x.FirstName) : query.OrderBy(x => x.FirstName),
            "lastname" => sortDesc ? query.OrderByDescending(x => x.LastName) : query.OrderBy(x => x.LastName),
            "email" => sortDesc ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
            "role" => sortDesc ? query.OrderByDescending(x => x.Role) : query.OrderBy(x => x.Role),
            _ => query.OrderByDescending(x => x.Id)
        };
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, total);
    }

    public Task<bool> UserIdentityExistsAsync(string tenantId, string username, string email, CancellationToken cancellationToken)
        => _db.Users.AnyAsync(x => x.TenantId == tenantId && !x.IsDeleted && (x.Username == username || x.Email == email), cancellationToken);

    public Task<CrmUser?> FindUserForSyncAsync(int crmUserId, CancellationToken cancellationToken)
        => _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == crmUserId, cancellationToken);

    public async Task<string?> GetContactNameAsync(string tenantId, int? contactId, CancellationToken cancellationToken)
    {
        var tenant = ModernCRM.SharedKernel.ValueObjects.TenantId.Create(tenantId);
        var contact = await _domainDb.Contacts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == contactId && x.TenantId == tenant && !x.IsDeleted, cancellationToken);
        return contact is null ? null : $"{contact.FirstName} {contact.LastName}".Trim();
    }

    public void Add(CrmUser entity) => _db.Users.Add(entity);
    public void Add(LeadModel entity) => _db.Leads.Add(entity);
    public void Add(ActivityModel entity) => _db.Activities.Add(entity);
}
