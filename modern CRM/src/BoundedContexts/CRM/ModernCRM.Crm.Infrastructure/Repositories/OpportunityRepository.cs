using ModernCRM.Crm.Domain.Opportunities;
using ModernCRM.Crm.Domain.Repositories;
using ModernCRM.Crm.Infrastructure.Persistence;
using ModernCRM.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ModernCRM.Crm.Infrastructure.Repositories;

public sealed class OpportunityRepository : IOpportunityRepository
{
    private readonly CrmDbContext _db;
    public OpportunityRepository(CrmDbContext db) => _db = db;

    public async Task AddAsync(Opportunity opportunity, CancellationToken ct) => await _db.Opportunities.AddAsync(opportunity, ct);

    public Task<Opportunity?> GetAsync(int id, CancellationToken ct) => _db.Opportunities.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Opportunity>> ListAsync(TenantId tenantId, string? stage, string? search, CancellationToken ct)
    {
        var query = _db.Opportunities.Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(stage) && Enum.TryParse<OpportunityStage>(stage, true, out var parsedStage))
            query = query.Where(x => x.Stage == parsedStage);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Title.Contains(term));
        }
        return await query.AsNoTracking().OrderBy(x => x.ExpectedCloseDate ?? DateTime.MaxValue).ThenBy(x => x.Title).ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

}
