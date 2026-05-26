using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Contacts.Commands;

public record DeleteContactCommand(int Id) : IRequest<bool>;

public class DeleteContactCommandHandler : IRequestHandler<DeleteContactCommand, bool>
{
    private readonly AppDbContext _db;
    public DeleteContactCommandHandler(AppDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteContactCommand r, CancellationToken ct)
    {
        var entity = await _db.Contacts.FirstOrDefaultAsync(c => c.Id == r.Id, ct);
        if (entity is null) return false;

        entity.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
