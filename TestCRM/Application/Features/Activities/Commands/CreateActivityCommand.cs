using MediatR;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Activities.Commands;

public record CreateActivityCommand(string Subject, ActivityType Type, string? Description, DateTime? DueDate, int? ContactId) : IRequest<int>;

public class CreateActivityCommandHandler : IRequestHandler<CreateActivityCommand, int>
{
    private readonly AppDbContext _db;
    public CreateActivityCommandHandler(AppDbContext db) => _db = db;

    public async Task<int> Handle(CreateActivityCommand r, CancellationToken ct)
    {
        var entity = new Activity { Subject = r.Subject, Type = r.Type, Description = r.Description, DueDate = r.DueDate, ContactId = r.ContactId };
        _db.Activities.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }
}
