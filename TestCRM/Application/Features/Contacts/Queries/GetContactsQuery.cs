using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Contacts.Queries;

public record ContactDto(
    int Id, string FirstName, string LastName,
    string Email, string? Phone, string? Company,
    string? JobTitle, string? Notes,
    DateTime CreatedAt, DateTime? UpdatedAt);

public record GetContactsQuery(
    int Page = 1, int PageSize = 20,
    string? SortBy = null, bool SortDesc = false,
    string? Search = null
) : IRequest<PagedResult<ContactDto>>;

public class GetContactsQueryHandler : IRequestHandler<GetContactsQuery, PagedResult<ContactDto>>
{
    private readonly AppDbContext _db;
    public GetContactsQueryHandler(AppDbContext db) => _db = db;

    public async Task<PagedResult<ContactDto>> Handle(GetContactsQuery r, CancellationToken ct)
    {
        var pageSize = Math.Min(r.PageSize, 100);
        var q = _db.Contacts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(r.Search))
        {
            var s = r.Search.Trim().ToLower();
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
            "lastname"  => r.SortDesc ? q.OrderByDescending(c => c.LastName)  : q.OrderBy(c => c.LastName),
            "email"     => r.SortDesc ? q.OrderByDescending(c => c.Email)     : q.OrderBy(c => c.Email),
            "company"   => r.SortDesc ? q.OrderByDescending(c => c.Company)   : q.OrderBy(c => c.Company),
            "phone"     => r.SortDesc ? q.OrderByDescending(c => c.Phone)     : q.OrderBy(c => c.Phone),
            _           => r.SortDesc ? q.OrderByDescending(c => c.FirstName) : q.OrderBy(c => c.FirstName),
        };

        var items = await q
            .Skip((r.Page - 1) * pageSize).Take(pageSize)
            .Select(c => new ContactDto(c.Id, c.FirstName, c.LastName, c.Email, c.Phone, c.Company, c.JobTitle, c.Notes, c.CreatedAt, c.UpdatedAt))
            .ToListAsync(ct);

        return new PagedResult<ContactDto>(items, total, r.Page, pageSize);
    }
}
