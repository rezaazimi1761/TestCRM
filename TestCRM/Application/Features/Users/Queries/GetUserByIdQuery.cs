using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Users.Queries;

public record GetUserByIdQuery(int Id) : IRequest<UserDto?>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly AppDbContext _db;
    public GetUserByIdQueryHandler(AppDbContext db) => _db = db;

    public Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken ct)
        => _db.Users
            .Where(u => u.Id == request.Id)
            .Select(u => new UserDto(u.Id, u.FirstName, u.LastName, u.Email, u.Role, u.IsActive, u.CreatedAt, u.UpdatedAt))
            .FirstOrDefaultAsync(ct);
}
