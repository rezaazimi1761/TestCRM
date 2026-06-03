using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Leads.Queries;

public record GetLeadsQuery(
    int Page = 1, int PageSize = 20,
    string? SortBy = null, bool SortDesc = false,
    string? Search = null,
    string? Status = null
) : IRequest<PagedResult<Lead>>;

public class GetLeadsQueryHandler : IRequestHandler<GetLeadsQuery, PagedResult<Lead>>
{
    private readonly AppDbContext _db;
    public GetLeadsQueryHandler(AppDbContext db) => _db = db;

    public async Task<PagedResult<Lead>> Handle(GetLeadsQuery r, CancellationToken ct)
    {
        var q = _db.Leads.AsQueryable();

        if (!string.IsNullOrWhiteSpace(r.Search))
        {
            var s = r.Search.ToLower();
            q = q.Where(l =>
                l.FirstName.ToLower().Contains(s) ||
                l.LastName.ToLower().Contains(s)  ||
                l.Email.ToLower().Contains(s)     ||
                (l.Company != null && l.Company.ToLower().Contains(s)) ||
                (l.Source  != null && l.Source.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(r.Status) &&
            Enum.TryParse<LeadStatus>(r.Status, out var status))
            q = q.Where(l => l.Status == status);

        var total = await q.CountAsync(ct);

        q = r.SortBy switch
        {
            "lastName" => r.SortDesc ? q.OrderByDescending(l => l.LastName) : q.OrderBy(l => l.LastName),
            "email"    => r.SortDesc ? q.OrderByDescending(l => l.Email)    : q.OrderBy(l => l.Email),
            "company"  => r.SortDesc ? q.OrderByDescending(l => l.Company)  : q.OrderBy(l => l.Company),
            "status"   => r.SortDesc ? q.OrderByDescending(l => l.Status)   : q.OrderBy(l => l.Status),
            "source"   => r.SortDesc ? q.OrderByDescending(l => l.Source)   : q.OrderBy(l => l.Source),
            _          => r.SortDesc ? q.OrderByDescending(l => l.FirstName): q.OrderBy(l => l.FirstName),
        };

        var items = await q.Skip((r.Page - 1) * r.PageSize).Take(r.PageSize).ToListAsync(ct);
        return new PagedResult<Lead>(items, total, r.Page, r.PageSize);
    }
}
