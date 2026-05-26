using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Leads.Commands;

public record UpdateLeadCommand(int Id, string FirstName, string LastName, string Email, string? Phone, string? Company, string? Source, LeadStatus Status, string? Notes) : IRequest<bool>;

public class UpdateLeadCommandHandler : IRequestHandler<UpdateLeadCommand, bool>
{
    private readonly AppDbContext _db;
    public UpdateLeadCommandHandler(AppDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateLeadCommand r, CancellationToken ct)
    {
        var entity = await _db.Leads.FirstOrDefaultAsync(l => l.Id == r.Id, ct);
        if (entity is null) return false;

        entity.FirstName = r.FirstName;
        entity.LastName = r.LastName;
        entity.Email = r.Email;
        entity.Phone = r.Phone;
        entity.Company = r.Company;
        entity.Source = r.Source;
        entity.Status = r.Status;
        entity.Notes = r.Notes;

        await _db.SaveChangesAsync(ct);
        return true;
    }
}
