using MediatR;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Leads.Commands;

public record CreateLeadCommand(string FirstName, string LastName, string Email, string? Phone, string? Company, string? Source, string? Notes) : IRequest<int>;

public class CreateLeadCommandHandler : IRequestHandler<CreateLeadCommand, int>
{
    private readonly AppDbContext _db;
    public CreateLeadCommandHandler(AppDbContext db) => _db = db;

    public async Task<int> Handle(CreateLeadCommand r, CancellationToken ct)
    {
        var entity = new Lead { FirstName = r.FirstName, LastName = r.LastName, Email = r.Email, Phone = r.Phone, Company = r.Company, Source = r.Source, Notes = r.Notes };
        _db.Leads.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }
}
