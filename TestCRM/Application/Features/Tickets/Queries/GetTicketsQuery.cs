using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Tickets.Queries;

public record TicketDto(
    int Id, string Subject, string? Description,
    TicketStatus Status, TicketPriority Priority,
    int? AccountId, string? AccountName,
    int? ContactId, int? AssignedToUserId,
    DateTime? DueDate, DateTime? ResolvedAt,
    string? Category, string? Notes);

public record GetTicketsQuery : IRequest<List<TicketDto>>;

public class GetTicketsQueryHandler : IRequestHandler<GetTicketsQuery, List<TicketDto>>
{
    private readonly AppDbContext _db;
    public GetTicketsQueryHandler(AppDbContext db) => _db = db;

    public Task<List<TicketDto>> Handle(GetTicketsQuery request, CancellationToken ct)
        => _db.Tickets
              .Include(t => t.Account)
              .Select(t => new TicketDto(
                  t.Id, t.Subject, t.Description,
                  t.Status, t.Priority,
                  t.AccountId, t.Account != null ? t.Account.Name : null,
                  t.ContactId, t.AssignedToUserId,
                  t.DueDate, t.ResolvedAt,
                  t.Category, t.Notes))
              .ToListAsync(ct);
}
