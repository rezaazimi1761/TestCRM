using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Users.Queries;

public record GetUsersQuery(
    int Page = 1, int PageSize = 20,
    string? SortBy = null, bool SortDesc = false,
    string? Search = null,
    string? Role = null
) : IRequest<PagedResult<AppUser>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<AppUser>>
{
    private readonly AppDbContext _db;
    public GetUsersQueryHandler(AppDbContext db) => _db = db;

    public async Task<PagedResult<AppUser>> Handle(GetUsersQuery r, CancellationToken ct)
    {
        var q = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(r.Search))
        {
            var s = r.Search.ToLower();
            q = q.Where(u =>
                u.FirstName.ToLower().Contains(s) ||
                u.LastName.ToLower().Contains(s)  ||
                u.Email.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(r.Role))
            q = q.Where(u => u.Role == r.Role);

        var total = await q.CountAsync(ct);

        q = r.SortBy switch
        {
            "lastName" => r.SortDesc ? q.OrderByDescending(u => u.LastName) : q.OrderBy(u => u.LastName),
            "email"    => r.SortDesc ? q.OrderByDescending(u => u.Email)    : q.OrderBy(u => u.Email),
            "role"     => r.SortDesc ? q.OrderByDescending(u => u.Role)     : q.OrderBy(u => u.Role),
            _          => r.SortDesc ? q.OrderByDescending(u => u.FirstName): q.OrderBy(u => u.FirstName),
        };

        var items = await q.Skip((r.Page - 1) * r.PageSize).Take(r.PageSize).ToListAsync(ct);
        return new PagedResult<AppUser>(items, total, r.Page, r.PageSize);
    }
}
