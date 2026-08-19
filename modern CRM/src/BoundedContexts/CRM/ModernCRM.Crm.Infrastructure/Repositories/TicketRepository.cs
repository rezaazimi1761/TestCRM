using ModernCRM.Crm.Domain.Repositories;
using ModernCRM.Crm.Domain.Tickets;
using ModernCRM.Crm.Infrastructure.Persistence;
using ModernCRM.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Infrastructure.Repositories;

public sealed class TicketRepository : ITicketRepository
{
    private readonly CrmDbContext _db;
    public TicketRepository(CrmDbContext db) { _db = db; UnitOfWork = new EfUnitOfWork(db); }
    public IUnitOfWork UnitOfWork { get; }

    public async Task AddAsync(Ticket ticket, CancellationToken ct) => await _db.Tickets.AddAsync(ticket, ct);

    public Task<Ticket?> GetAsync(TenantId tenantId, int id, CancellationToken ct) => _db.Tickets.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

    public async Task<IReadOnlyList<Ticket>> ListAsync(TenantId tenantId, string? status, string? priority, CancellationToken ct)
    {
        var query = _db.Tickets.Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TicketStatus>(status, true, out var parsedStatus))
            query = query.Where(x => x.Status == parsedStatus);
        if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<TicketPriority>(priority, true, out var parsedPriority))
            query = query.Where(x => x.Priority == parsedPriority);
        return await query.AsNoTracking().OrderBy(x => x.DueDate ?? DateTime.MaxValue).ThenBy(x => x.Id).ToListAsync(ct);
    }

}
