using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Users.Commands;

public record DeleteUserCommand(int Id) : IRequest<bool>;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly AppDbContext _db;
    public DeleteUserCommandHandler(AppDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteUserCommand r, CancellationToken ct)
    {
        var entity = await _db.Users.FirstOrDefaultAsync(u => u.Id == r.Id, ct);
        if (entity is null) return false;

        entity.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
