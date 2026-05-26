using MediatR;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Accounts.Commands;

public record CreateAccountCommand(string Name, string? Industry, string? Website, string? Phone, string? Address, string? Notes) : IRequest<int>;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, int>
{
    private readonly AppDbContext _db;
    public CreateAccountCommandHandler(AppDbContext db) => _db = db;

    public async Task<int> Handle(CreateAccountCommand r, CancellationToken ct)
    {
        var entity = new Account { Name = r.Name, Industry = r.Industry, Website = r.Website, Phone = r.Phone, Address = r.Address, Notes = r.Notes };
        _db.Accounts.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }
}
