using MediatR;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Opportunities.Commands;

public record CreateOpportunityCommand(string Title, decimal Value, OpportunityStage Stage, DateTime? ExpectedCloseDate, int? ContactId, int? AccountId, string? Notes) : IRequest<int>;

public class CreateOpportunityCommandHandler : IRequestHandler<CreateOpportunityCommand, int>
{
    private readonly AppDbContext _db;
    public CreateOpportunityCommandHandler(AppDbContext db) => _db = db;

    public async Task<int> Handle(CreateOpportunityCommand r, CancellationToken ct)
    {
        var entity = new Opportunity { Title = r.Title, Value = r.Value, Stage = r.Stage, ExpectedCloseDate = r.ExpectedCloseDate, ContactId = r.ContactId, AccountId = r.AccountId, Notes = r.Notes };
        _db.Opportunities.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }
}
