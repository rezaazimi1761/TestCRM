using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Opportunities.Queries;

public record GetOpportunityByIdQuery(int Id) : IRequest<Opportunity?>;

public class GetOpportunityByIdQueryHandler : IRequestHandler<GetOpportunityByIdQuery, Opportunity?>
{
    private readonly AppDbContext _db;
    public GetOpportunityByIdQueryHandler(AppDbContext db) => _db = db;

    public Task<Opportunity?> Handle(GetOpportunityByIdQuery request, CancellationToken ct)
        => _db.Opportunities.FirstOrDefaultAsync(o => o.Id == request.Id, ct);
}
