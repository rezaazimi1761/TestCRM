using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Contacts.Commands;

public record UpdateContactCommand(int Id, string FirstName, string LastName, string Email, string? Phone, string? Company, string? JobTitle, string? Notes) : IRequest<bool>;

public class UpdateContactCommandHandler : IRequestHandler<UpdateContactCommand, bool>
{
    private readonly AppDbContext _db;
    public UpdateContactCommandHandler(AppDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateContactCommand r, CancellationToken ct)
    {
        var entity = await _db.Contacts.FirstOrDefaultAsync(c => c.Id == r.Id, ct);
        if (entity is null) return false;

        entity.FirstName = r.FirstName;
        entity.LastName = r.LastName;
        entity.Email = r.Email;
        entity.Phone = r.Phone;
        entity.Company = r.Company;
        entity.JobTitle = r.JobTitle;
        entity.Notes = r.Notes;

        await _db.SaveChangesAsync(ct);
        return true;
    }
}
