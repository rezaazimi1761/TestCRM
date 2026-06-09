using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
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

public record GetTicketsQuery(
    int Page = 1, int PageSize = 20,
    string? SortBy = null, bool SortDesc = false,
    string? Search = null,
    string? Status = null,
    string? Priority = null
) : IRequest<PagedResult<TicketDto>>;

public class GetTicketsQueryHandler : IRequestHandler<GetTicketsQuery, PagedResult<TicketDto>>
{
    private readonly AppDbContext _db;
    public GetTicketsQueryHandler(AppDbContext db) => _db = db;

    public async Task<PagedResult<TicketDto>> Handle(GetTicketsQuery r, CancellationToken ct)
    {
        var pageSize = Math.Min(r.PageSize, 100);
        var q = _db.Tickets
            .Include(t => t.Account)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(r.Search))
        {
            var s = r.Search.ToLower();
            q = q.Where(t =>
                t.Subject.ToLower().Contains(s) ||
                (t.Category    != null && t.Category.ToLower().Contains(s)) ||
                (t.Description != null && t.Description.ToLower().Contains(s)) ||
                (t.Account     != null && t.Account.Name.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(r.Status) &&
            Enum.TryParse<TicketStatus>(r.Status, out var status))
            q = q.Where(t => t.Status == status);

        if (!string.IsNullOrWhiteSpace(r.Priority) &&
            Enum.TryParse<TicketPriority>(r.Priority, out var priority))
            q = q.Where(t => t.Priority == priority);

        var total = await q.CountAsync(ct);

        q = r.SortBy switch
        {
            "status"   => r.SortDesc ? q.OrderByDescending(t => t.Status)   : q.OrderBy(t => t.Status),
            "priority" => r.SortDesc ? q.OrderByDescending(t => t.Priority) : q.OrderBy(t => t.Priority),
            "dueDate"  => r.SortDesc ? q.OrderByDescending(t => t.DueDate)  : q.OrderBy(t => t.DueDate),
            "category" => r.SortDesc ? q.OrderByDescending(t => t.Category) : q.OrderBy(t => t.Category),
            _          => r.SortDesc ? q.OrderByDescending(t => t.Subject)  : q.OrderBy(t => t.Subject),
        };

        var items = await q
            .Skip((r.Page - 1) * pageSize).Take(pageSize)
            .Select(t => new TicketDto(
                t.Id, t.Subject, t.Description,
                t.Status, t.Priority,
                t.AccountId, t.Account != null ? t.Account.Name : null,
                t.ContactId, t.AssignedToUserId,
                t.DueDate, t.ResolvedAt,
                t.Category, t.Notes))
            .ToListAsync(ct);

        return new PagedResult<TicketDto>(items, total, r.Page, pageSize);
    }
}
