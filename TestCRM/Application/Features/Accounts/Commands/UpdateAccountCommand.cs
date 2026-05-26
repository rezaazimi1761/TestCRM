using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Accounts.Commands;

public record UpdateAccountCommand(int Id, string Name, string? Industry, string? Website, string? Phone, string? Address, string? Notes) : IRequest<bool>;

public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, bool>
{
    private readonly AppDbContext _db;
    public UpdateAccountCommandHandler(AppDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateAccountCommand r, CancellationToken ct)
    {
        var entity = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == r.Id, ct);
        if (entity is null) return false;

        entity.Name = r.Name;
        entity.Industry = r.Industry;
        entity.Website = r.Website;
        entity.Phone = r.Phone;
        entity.Address = r.Address;
        entity.Notes = r.Notes;

        await _db.SaveChangesAsync(ct);
        return true;
    }
}
