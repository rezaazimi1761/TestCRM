using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Opportunities.Commands;

public record UpdateOpportunityCommand(int Id, string Title, decimal Value, OpportunityStage Stage, DateTime? ExpectedCloseDate, int? ContactId, int? AccountId, string? Notes) : IRequest<bool>;

public class UpdateOpportunityCommandHandler : IRequestHandler<UpdateOpportunityCommand, bool>
{
    private readonly AppDbContext _db;
    public UpdateOpportunityCommandHandler(AppDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateOpportunityCommand r, CancellationToken ct)
    {
        var entity = await _db.Opportunities.FirstOrDefaultAsync(o => o.Id == r.Id, ct);
        if (entity is null) return false;

        entity.Title = r.Title;
        entity.Value = r.Value;
        entity.Stage = r.Stage;
        entity.ExpectedCloseDate = r.ExpectedCloseDate;
        entity.ContactId = r.ContactId;
        entity.AccountId = r.AccountId;
        entity.Notes = r.Notes;

        await _db.SaveChangesAsync(ct);
        return true;
    }
}
