using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Users.Commands;

public record UpdateUserCommand(int Id, string FirstName, string LastName, string Email, string Role, bool IsActive) : IRequest<bool>;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, bool>
{
    private readonly AppDbContext _db;
    public UpdateUserCommandHandler(AppDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateUserCommand r, CancellationToken ct)
    {
        var entity = await _db.Users.FirstOrDefaultAsync(u => u.Id == r.Id, ct);
        if (entity is null) return false;

        entity.FirstName = r.FirstName;
        entity.LastName = r.LastName;
        entity.Email = r.Email;
        entity.Role = r.Role;
        entity.IsActive = r.IsActive;

        await _db.SaveChangesAsync(ct);
        return true;
    }
}
