using ModernCRM.Crm.Domain.Repositories;
using ModernCRM.Crm.Domain.Tickets;
using ModernCRM.Crm.Infrastructure.Persistence;
using ModernCRM.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ModernCRM.Crm.Infrastructure.Repositories;

public sealed class TicketRepository : ITicketRepository
{
    private readonly CrmDbContext _db;
    public TicketRepository(CrmDbContext db) => _db = db;

    public async Task AddAsync(Ticket ticket, CancellationToken ct) => await _db.Tickets.AddAsync(ticket, ct);

    public Task<Ticket?> GetAsync(int id, CancellationToken ct) => _db.Tickets.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Ticket>> ListAsync(TenantId tenantId, string? status, string? priority, CancellationToken ct)
    {
        var query = _db.Tickets.Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TicketStatus>(status, true, out var parsedStatus))
            query = query.Where(x => x.Status == parsedStatus);
        if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<TicketPriority>(priority, true, out var parsedPriority))
            query = query.Where(x => x.Priority == parsedPriority);
        return await query.AsNoTracking().OrderBy(x => x.DueDate ?? DateTime.MaxValue).ThenBy(x => x.Id).ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

}
