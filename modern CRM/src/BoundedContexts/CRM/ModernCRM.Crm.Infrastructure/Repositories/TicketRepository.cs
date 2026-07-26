using ModernCRM.Crm.Domain.Repositories;
using ModernCRM.Crm.Domain.Tickets;
using ModernCRM.Crm.Infrastructure.Persistence;
using ModernCRM.SharedKernel.BuildingBlocks;
using ModernCRM.SharedKernel.ValueObjects;
using System.Reflection;

namespace ModernCRM.Crm.Infrastructure.Repositories;

public sealed class TicketRepository : ITicketRepository
{
    private readonly CrmDbContext _db;
    public TicketRepository(CrmDbContext db) => _db = db;

    public Task AddAsync(Ticket ticket, CancellationToken ct)
    {
        if (ticket.Id == 0) SetId(ticket, _db.NextTicketId());
        _db.Tickets.Add(ticket);
        return Task.CompletedTask;
    }

    public Task<Ticket?> GetAsync(int id, CancellationToken ct) => Task.FromResult(_db.Tickets.FirstOrDefault(x => x.Id == id));

    public Task<IReadOnlyList<Ticket>> ListAsync(TenantId tenantId, string? status, string? priority, CancellationToken ct)
    {
        IEnumerable<Ticket> query = _db.Tickets.Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TicketStatus>(status, true, out var parsedStatus))
            query = query.Where(x => x.Status == parsedStatus);
        if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<TicketPriority>(priority, true, out var parsedPriority))
            query = query.Where(x => x.Priority == parsedPriority);
        return Task.FromResult<IReadOnlyList<Ticket>>(query.OrderBy(x => x.DueDate ?? DateTime.MaxValue).ThenBy(x => x.Id).ToList());
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

    private static void SetId<TEntity>(TEntity entity, int id) where TEntity : Entity<int>
        => typeof(Entity<int>).GetProperty(nameof(Entity<int>.Id), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(entity, id);
}