using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Leads.Commands;

public record DeleteLeadCommand(int Id) : IRequest<bool>;

public class DeleteLeadCommandHandler : IRequestHandler<DeleteLeadCommand, bool>
{
    private readonly AppDbContext _db;
    public DeleteLeadCommandHandler(AppDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteLeadCommand r, CancellationToken ct)
    {
        var entity = await _db.Leads.FirstOrDefaultAsync(l => l.Id == r.Id, ct);
        if (entity is null) return false;

        entity.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
