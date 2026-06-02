using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Tickets.Commands;

public record UpdateTicketCommand(
    int Id,
    string Subject,
    string? Description,
    TicketStatus Status,
    TicketPriority Priority,
    int? AccountId,
    int? ContactId,
    int? AssignedToUserId,
    DateTime? DueDate,
    DateTime? ResolvedAt,
    string? Category,
    string? Notes
) : IRequest<bool>;

public class UpdateTicketCommandHandler : IRequestHandler<UpdateTicketCommand, bool>
{
    private readonly AppDbContext _db;
    public UpdateTicketCommandHandler(AppDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateTicketCommand r, CancellationToken ct)
    {
        var entity = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == r.Id, ct);
        if (entity is null) return false;

        entity.Subject          = r.Subject;
        entity.Description      = r.Description;
        entity.Status           = r.Status;
        entity.Priority         = r.Priority;
        entity.AccountId        = r.AccountId;
        entity.ContactId        = r.ContactId;
        entity.AssignedToUserId = r.AssignedToUserId;
        entity.DueDate          = r.DueDate;
        entity.ResolvedAt       = r.ResolvedAt;
        entity.Category         = r.Category;
        entity.Notes            = r.Notes;

        await _db.SaveChangesAsync(ct);
        return true;
    }
}
