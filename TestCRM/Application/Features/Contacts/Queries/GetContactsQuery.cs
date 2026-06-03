using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Contacts.Queries;

public record GetContactsQuery(
    int Page = 1, int PageSize = 20,
    string? SortBy = null, bool SortDesc = false,
    string? Search = null
) : IRequest<PagedResult<Contact>>;

public class GetContactsQueryHandler : IRequestHandler<GetContactsQuery, PagedResult<Contact>>
{
    private readonly AppDbContext _db;
    public GetContactsQueryHandler(AppDbContext db) => _db = db;

    public async Task<PagedResult<Contact>> Handle(GetContactsQuery r, CancellationToken ct)
    {
        var q = _db.Contacts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(r.Search))
        {
            var s = r.Search.ToLower();
            q = q.Where(c =>
                c.FirstName.ToLower().Contains(s) ||
                c.LastName.ToLower().Contains(s)  ||
                c.Email.ToLower().Contains(s)     ||
                (c.Company != null && c.Company.ToLower().Contains(s)) ||
                (c.Phone   != null && c.Phone.Contains(s)));
        }

        var total = await q.CountAsync(ct);

        q = r.SortBy switch
        {
            "lastName"  => r.SortDesc ? q.OrderByDescending(c => c.LastName)  : q.OrderBy(c => c.LastName),
            "email"     => r.SortDesc ? q.OrderByDescending(c => c.Email)     : q.OrderBy(c => c.Email),
            "company"   => r.SortDesc ? q.OrderByDescending(c => c.Company)   : q.OrderBy(c => c.Company),
            "phone"     => r.SortDesc ? q.OrderByDescending(c => c.Phone)     : q.OrderBy(c => c.Phone),
            _           => r.SortDesc ? q.OrderByDescending(c => c.FirstName) : q.OrderBy(c => c.FirstName),
        };

        var items = await q.Skip((r.Page - 1) * r.PageSize).Take(r.PageSize).ToListAsync(ct);
        return new PagedResult<Contact>(items, total, r.Page, r.PageSize);
    }
}
