using MediatR;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Contacts.Commands;

public record CreateContactCommand(string FirstName, string LastName, string Email, string? Phone, string? Company, string? JobTitle, string? Notes) : IRequest<int>;

public class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, int>
{
    private readonly AppDbContext _db;
    public CreateContactCommandHandler(AppDbContext db) => _db = db;

    public async Task<int> Handle(CreateContactCommand r, CancellationToken ct)
    {
        var entity = new Contact { FirstName = r.FirstName, LastName = r.LastName, Email = r.Email, Phone = r.Phone, Company = r.Company, JobTitle = r.JobTitle, Notes = r.Notes };
        _db.Contacts.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }
}
