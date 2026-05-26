using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Activities.Commands;

public record DeleteActivityCommand(int Id) : IRequest<bool>;

public class DeleteActivityCommandHandler : IRequestHandler<DeleteActivityCommand, bool>
{
    private readonly AppDbContext _db;
    public DeleteActivityCommandHandler(AppDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteActivityCommand r, CancellationToken ct)
    {
        var entity = await _db.Activities.FirstOrDefaultAsync(a => a.Id == r.Id, ct);
        if (entity is null) return false;

        entity.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
