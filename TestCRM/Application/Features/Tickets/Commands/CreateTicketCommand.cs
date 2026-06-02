using MediatR;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Tickets.Commands;

public record CreateTicketCommand(
    string Subject,
    string? Description,
    TicketStatus Status,
    TicketPriority Priority,
    int? AccountId,
    int? ContactId,
    int? AssignedToUserId,
    DateTime? DueDate,
    string? Category,
    string? Notes
) : IRequest<int>;

public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, int>
{
    private readonly AppDbContext _db;
    public CreateTicketCommandHandler(AppDbContext db) => _db = db;

    public async Task<int> Handle(CreateTicketCommand r, CancellationToken ct)
    {
        var entity = new Ticket
        {
            Subject          = r.Subject,
            Description      = r.Description,
            Status           = r.Status,
            Priority         = r.Priority,
            AccountId        = r.AccountId,
            ContactId        = r.ContactId,
            AssignedToUserId = r.AssignedToUserId,
            DueDate          = r.DueDate,
            Category         = r.Category,
            Notes            = r.Notes
        };
        _db.Tickets.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }
}
