using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Activities.Queries;

public record GetActivityByIdQuery(int Id) : IRequest<Activity?>;

public class GetActivityByIdQueryHandler : IRequestHandler<GetActivityByIdQuery, Activity?>
{
    private readonly AppDbContext _db;
    public GetActivityByIdQueryHandler(AppDbContext db) => _db = db;

    public Task<Activity?> Handle(GetActivityByIdQuery request, CancellationToken ct)
        => _db.Activities.FirstOrDefaultAsync(a => a.Id == request.Id, ct);
}
