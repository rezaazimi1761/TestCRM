using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Activities.Queries;

public record ActivityDto(
    int Id, string Subject, string Type, string? Description,
    int? ContactId, string? ContactName,
    int? AssignedToUserId, DateTime? DueDate, bool IsCompleted);

public record GetActivitiesQuery(
    int Page = 1, int PageSize = 20,
    string? SortBy = null, bool SortDesc = false,
    string? Search = null,
    string? Type = null,
    bool? IsCompleted = null
) : IRequest<PagedResult<ActivityDto>>;

public class GetActivitiesQueryHandler : IRequestHandler<GetActivitiesQuery, PagedResult<ActivityDto>>
{
    private readonly AppDbContext _db;
    public GetActivitiesQueryHandler(AppDbContext db) => _db = db;

    public async Task<PagedResult<ActivityDto>> Handle(GetActivitiesQuery r, CancellationToken ct)
    {
        var pageSize = Math.Min(r.PageSize, 100);
        var q = _db.Activities
            .Include(a => a.Contact)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(r.Search))
        {
            var s = r.Search.ToLower();
            q = q.Where(a =>
                a.Subject.ToLower().Contains(s) ||
                (a.Description != null && a.Description.ToLower().Contains(s)) ||
                (a.Contact != null && (a.Contact.FirstName + " " + a.Contact.LastName).ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(r.Type) &&
            Enum.TryParse<ActivityType>(r.Type, out var type))
            q = q.Where(a => a.Type == type);

        if (r.IsCompleted.HasValue)
            q = q.Where(a => a.IsCompleted == r.IsCompleted.Value);

        var total = await q.CountAsync(ct);

        q = r.SortBy switch
        {
            "type"        => r.SortDesc ? q.OrderByDescending(a => a.Type)        : q.OrderBy(a => a.Type),
            "dueDate"     => r.SortDesc ? q.OrderByDescending(a => a.DueDate)     : q.OrderBy(a => a.DueDate),
            "isCompleted" => r.SortDesc ? q.OrderByDescending(a => a.IsCompleted) : q.OrderBy(a => a.IsCompleted),
            _             => r.SortDesc ? q.OrderByDescending(a => a.Subject)     : q.OrderBy(a => a.Subject),
        };

        var items = await q
            .Skip((r.Page - 1) * pageSize).Take(pageSize)
            .Select(a => new ActivityDto(
                a.Id, a.Subject, a.Type.ToString(), a.Description,
                a.ContactId, a.Contact != null ? a.Contact.FirstName + " " + a.Contact.LastName : null,
                a.AssignedToUserId, a.DueDate, a.IsCompleted))
            .ToListAsync(ct);

        return new PagedResult<ActivityDto>(items, total, r.Page, pageSize);
    }
}
