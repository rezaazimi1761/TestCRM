using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Activities.Queries;

public record GetActivitiesQuery : IRequest<List<Activity>>;

public class GetActivitiesQueryHandler : IRequestHandler<GetActivitiesQuery, List<Activity>>
{
    private readonly AppDbContext _db;
    public GetActivitiesQueryHandler(AppDbContext db) => _db = db;

    public Task<List<Activity>> Handle(GetActivitiesQuery request, CancellationToken ct)
        => _db.Activities.ToListAsync(ct);
}
