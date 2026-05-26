using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Opportunities.Queries;

public record GetOpportunitiesQuery : IRequest<List<Opportunity>>;

public class GetOpportunitiesQueryHandler : IRequestHandler<GetOpportunitiesQuery, List<Opportunity>>
{
    private readonly AppDbContext _db;
    public GetOpportunitiesQueryHandler(AppDbContext db) => _db = db;

    public Task<List<Opportunity>> Handle(GetOpportunitiesQuery request, CancellationToken ct)
        => _db.Opportunities.ToListAsync(ct);
}
