using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Activities.Commands;

public record UpdateActivityCommand(int Id, string Subject, ActivityType Type, string? Description, DateTime? DueDate, bool IsCompleted, int? ContactId) : IRequest<bool>;

public class UpdateActivityCommandHandler : IRequestHandler<UpdateActivityCommand, bool>
{
    private readonly AppDbContext _db;
    public UpdateActivityCommandHandler(AppDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateActivityCommand r, CancellationToken ct)
    {
        var entity = await _db.Activities.FirstOrDefaultAsync(a => a.Id == r.Id, ct);
        if (entity is null) return false;

        entity.Subject = r.Subject;
        entity.Type = r.Type;
        entity.Description = r.Description;
        entity.DueDate = r.DueDate;
        entity.IsCompleted = r.IsCompleted;
        entity.ContactId = r.ContactId;

        await _db.SaveChangesAsync(ct);
        return true;
    }
}
