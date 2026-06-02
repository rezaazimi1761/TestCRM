using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Tickets.Queries;

public record GetTicketByIdQuery(int Id) : IRequest<TicketDto?>;

public class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, TicketDto?>
{
    private readonly AppDbContext _db;
    public GetTicketByIdQueryHandler(AppDbContext db) => _db = db;

    public Task<TicketDto?> Handle(GetTicketByIdQuery request, CancellationToken ct)
        => _db.Tickets
              .Include(t => t.Account)
              .Where(t => t.Id == request.Id)
              .Select(t => new TicketDto(
                  t.Id, t.Subject, t.Description,
                  t.Status, t.Priority,
                  t.AccountId, t.Account != null ? t.Account.Name : null,
                  t.ContactId, t.AssignedToUserId,
                  t.DueDate, t.ResolvedAt,
                  t.Category, t.Notes))
              .FirstOrDefaultAsync(ct);
}
