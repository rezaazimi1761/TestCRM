using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Opportunities.Commands;

public record DeleteOpportunityCommand(int Id) : IRequest<bool>;

public class DeleteOpportunityCommandHandler : IRequestHandler<DeleteOpportunityCommand, bool>
{
    private readonly AppDbContext _db;
    public DeleteOpportunityCommandHandler(AppDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteOpportunityCommand r, CancellationToken ct)
    {
        var entity = await _db.Opportunities.FirstOrDefaultAsync(o => o.Id == r.Id, ct);
        if (entity is null) return false;

        entity.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
