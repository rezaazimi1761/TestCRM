using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Users.Queries;

public record GetUserByIdQuery(int Id) : IRequest<AppUser?>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, AppUser?>
{
    private readonly AppDbContext _db;
    public GetUserByIdQueryHandler(AppDbContext db) => _db = db;

    public Task<AppUser?> Handle(GetUserByIdQuery request, CancellationToken ct)
        => _db.Users.FirstOrDefaultAsync(u => u.Id == request.Id, ct);
}
