using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Opportunities.Queries;

public record OpportunityDto(
    int Id, string Title, decimal Value, string Stage,
    int? AccountId, string? AccountName,
    int? ContactId, string? ContactName,
    int? AssignedToUserId, DateTime? ExpectedCloseDate, string? Notes);

public record GetOpportunitiesQuery(
    int Page = 1, int PageSize = 20,
    string? SortBy = null, bool SortDesc = false,
    string? Search = null,
    string? Stage = null
) : IRequest<PagedResult<OpportunityDto>>;

public class GetOpportunitiesQueryHandler : IRequestHandler<GetOpportunitiesQuery, PagedResult<OpportunityDto>>
{
    private readonly AppDbContext _db;
    public GetOpportunitiesQueryHandler(AppDbContext db) => _db = db;

    public async Task<PagedResult<OpportunityDto>> Handle(GetOpportunitiesQuery r, CancellationToken ct)
    {
        var q = _db.Opportunities
            .Include(o => o.Account)
            .Include(o => o.Contact)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(r.Search))
        {
            var s = r.Search.ToLower();
            q = q.Where(o =>
                o.Title.ToLower().Contains(s) ||
                (o.Account != null && o.Account.Name.ToLower().Contains(s)) ||
                (o.Contact != null && (o.Contact.FirstName + " " + o.Contact.LastName).ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(r.Stage) &&
            Enum.TryParse<OpportunityStage>(r.Stage, out var stage))
            q = q.Where(o => o.Stage == stage);

        var total = await q.CountAsync(ct);

        q = r.SortBy switch
        {
            "value"             => r.SortDesc ? q.OrderByDescending(o => o.Value)             : q.OrderBy(o => o.Value),
            "stage"             => r.SortDesc ? q.OrderByDescending(o => o.Stage)             : q.OrderBy(o => o.Stage),
            "expectedCloseDate" => r.SortDesc ? q.OrderByDescending(o => o.ExpectedCloseDate) : q.OrderBy(o => o.ExpectedCloseDate),
            _                   => r.SortDesc ? q.OrderByDescending(o => o.Title)             : q.OrderBy(o => o.Title),
        };

        var items = await q
            .Skip((r.Page - 1) * r.PageSize).Take(r.PageSize)
            .Select(o => new OpportunityDto(
                o.Id, o.Title, o.Value, o.Stage.ToString(),
                o.AccountId, o.Account != null ? o.Account.Name : null,
                o.ContactId, o.Contact != null ? o.Contact.FirstName + " " + o.Contact.LastName : null,
                o.AssignedToUserId, o.ExpectedCloseDate, o.Notes))
            .ToListAsync(ct);

        return new PagedResult<OpportunityDto>(items, total, r.Page, r.PageSize);
    }
}
