using TestCRM.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Leads.Queries;

public record LeadDto(
    int Id, string FirstName, string LastName,
    string Email, string? Phone, string? Company,
    string? Source, LeadStatus Status, string? Notes,
    int? AssignedToUserId, DateTime CreatedAt, DateTime? UpdatedAt);

public record GetLeadsQuery(
    int Page = 1, int PageSize = 20,
    string? SortBy = null, bool SortDesc = false,
    string? Search = null,
    string? Status = null
) : IRequest<PagedResult<LeadDto>>;

public class GetLeadsQueryHandler : IRequestHandler<GetLeadsQuery, PagedResult<LeadDto>>
{
    private readonly AppDbContext _db;
    public GetLeadsQueryHandler(AppDbContext db) => _db = db;

    public async Task<PagedResult<LeadDto>> Handle(GetLeadsQuery r, CancellationToken ct)
    {
        var pageSize = Math.Min(r.PageSize, PaginationValidator.MaxPageSize);
        var q = _db.Leads.AsQueryable();

        if (!string.IsNullOrWhiteSpace(r.Search))
        {
            var s = r.Search.Trim().ToLower();
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
            "lastname"  => r.SortDesc ? q.OrderByDescending(l => l.LastName)  : q.OrderBy(l => l.LastName),
            "email"     => r.SortDesc ? q.OrderByDescending(l => l.Email)     : q.OrderBy(l => l.Email),
            "company"   => r.SortDesc ? q.OrderByDescending(l => l.Company)   : q.OrderBy(l => l.Company),
            "status"    => r.SortDesc ? q.OrderByDescending(l => l.Status)    : q.OrderBy(l => l.Status),
            "source"    => r.SortDesc ? q.OrderByDescending(l => l.Source)    : q.OrderBy(l => l.Source),
            _           => r.SortDesc ? q.OrderByDescending(l => l.FirstName) : q.OrderBy(l => l.FirstName),
        };

        var items = await q
            .Skip((r.Page - 1) * pageSize).Take(pageSize)
            .Select(l => new LeadDto(l.Id, l.FirstName, l.LastName, l.Email, l.Phone, l.Company, l.Source, l.Status, l.Notes, l.AssignedToUserId, l.CreatedAt, l.UpdatedAt))
            .ToListAsync(ct);

        return new PagedResult<LeadDto>(items, total, r.Page, pageSize);
    }
}
