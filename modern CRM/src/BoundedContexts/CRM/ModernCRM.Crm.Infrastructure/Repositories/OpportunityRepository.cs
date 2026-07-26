using ModernCRM.Crm.Domain.Opportunities;
using ModernCRM.Crm.Domain.Repositories;
using ModernCRM.Crm.Infrastructure.Persistence;
using ModernCRM.SharedKernel.BuildingBlocks;
using ModernCRM.SharedKernel.ValueObjects;
using System.Reflection;

namespace ModernCRM.Crm.Infrastructure.Repositories;

public sealed class OpportunityRepository : IOpportunityRepository
{
    private readonly CrmDbContext _db;
    public OpportunityRepository(CrmDbContext db) => _db = db;

    public Task AddAsync(Opportunity opportunity, CancellationToken ct)
    {
        if (opportunity.Id == 0) SetId(opportunity, _db.NextOpportunityId());
        _db.Opportunities.Add(opportunity);
        return Task.CompletedTask;
    }

    public Task<Opportunity?> GetAsync(int id, CancellationToken ct) => Task.FromResult(_db.Opportunities.FirstOrDefault(x => x.Id == id));

    public Task<IReadOnlyList<Opportunity>> ListAsync(TenantId tenantId, string? stage, string? search, CancellationToken ct)
    {
        IEnumerable<Opportunity> query = _db.Opportunities.Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(stage) && Enum.TryParse<OpportunityStage>(stage, true, out var parsedStage))
            query = query.Where(x => x.Stage == parsedStage);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Title.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
        return Task.FromResult<IReadOnlyList<Opportunity>>(query.OrderBy(x => x.ExpectedCloseDate ?? DateTime.MaxValue).ThenBy(x => x.Title).ToList());
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

    private static void SetId<TEntity>(TEntity entity, int id) where TEntity : Entity<int>
        => typeof(Entity<int>).GetProperty(nameof(Entity<int>.Id), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(entity, id);
}