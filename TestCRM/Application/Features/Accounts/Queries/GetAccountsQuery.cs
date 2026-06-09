using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Accounts.Queries;

public record AccountDto(
    int Id, string Name, string? Industry,
    string? Website, string? Phone, string? Address,
    string? Notes, DateTime CreatedAt, DateTime? UpdatedAt);

public record GetAccountsQuery(
    int Page = 1, int PageSize = 20,
    string? SortBy = null, bool SortDesc = false,
    string? Search = null
) : IRequest<PagedResult<AccountDto>>;

public class GetAccountsQueryHandler : IRequestHandler<GetAccountsQuery, PagedResult<AccountDto>>
{
    private readonly AppDbContext _db;
    public GetAccountsQueryHandler(AppDbContext db) => _db = db;

    public async Task<PagedResult<AccountDto>> Handle(GetAccountsQuery r, CancellationToken ct)
    {
        var pageSize = Math.Min(r.PageSize, 100);
        var q = _db.Accounts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(r.Search))
        {
            var s = r.Search.Trim().ToLower();
            q = q.Where(a =>
                a.Name.ToLower().Contains(s) ||
                (a.Industry != null && a.Industry.ToLower().Contains(s)) ||
                (a.Website  != null && a.Website.ToLower().Contains(s))  ||
                (a.Phone    != null && a.Phone.Contains(s)));
        }

        var total = await q.CountAsync(ct);

        q = r.SortBy switch
        {
            "industry" => r.SortDesc ? q.OrderByDescending(a => a.Industry) : q.OrderBy(a => a.Industry),
            "website"  => r.SortDesc ? q.OrderByDescending(a => a.Website)  : q.OrderBy(a => a.Website),
            "phone"    => r.SortDesc ? q.OrderByDescending(a => a.Phone)    : q.OrderBy(a => a.Phone),
            _          => r.SortDesc ? q.OrderByDescending(a => a.Name)     : q.OrderBy(a => a.Name),
        };

        var items = await q
            .Skip((r.Page - 1) * pageSize).Take(pageSize)
            .Select(a => new AccountDto(a.Id, a.Name, a.Industry, a.Website, a.Phone, a.Address, a.Notes, a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);

        return new PagedResult<AccountDto>(items, total, r.Page, pageSize);
    }
}
