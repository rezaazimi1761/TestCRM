using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Accounts.Commands;

public record DeleteAccountCommand(int Id) : IRequest<bool>;

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, bool>
{
    private readonly AppDbContext _db;
    public DeleteAccountCommandHandler(AppDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteAccountCommand r, CancellationToken ct)
    {
        var entity = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == r.Id, ct);
        if (entity is null) return false;

        entity.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
