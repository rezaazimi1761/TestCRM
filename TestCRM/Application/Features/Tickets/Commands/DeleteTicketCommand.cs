using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Tickets.Commands;

public record DeleteTicketCommand(int Id) : IRequest<bool>;

public class DeleteTicketCommandHandler : IRequestHandler<DeleteTicketCommand, bool>
{
    private readonly AppDbContext _db;
    public DeleteTicketCommandHandler(AppDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteTicketCommand r, CancellationToken ct)
    {
        var entity = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == r.Id, ct);
        if (entity is null) return false;

        entity.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
