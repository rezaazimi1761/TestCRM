using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Users.Queries;

public record GetUsersQuery : IRequest<List<AppUser>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<AppUser>>
{
    private readonly AppDbContext _db;

    public GetUsersQueryHandler(AppDbContext db) => _db = db;

    public Task<List<AppUser>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        => _db.Users.ToListAsync(cancellationToken);
}
