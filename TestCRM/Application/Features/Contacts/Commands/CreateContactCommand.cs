using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Application.Common;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Contacts.Commands;

public record CreateContactCommand(
    [property: Required(AllowEmptyStrings = false)] [property: StringLength(100)] string FirstName,
    [property: Required(AllowEmptyStrings = false)] [property: StringLength(100)] string LastName,
    [property: Required(AllowEmptyStrings = false)] [property: EmailAddress] [property: StringLength(255)] string Email,
    [property: StringLength(50)]  string? Phone,
    [property: StringLength(255)] string? Company,
    [property: StringLength(150)] string? JobTitle,
    [property: StringLength(2000)] string? Notes
) : IRequest<int>;

public class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, int>
{
    private readonly AppDbContext _db;
    public CreateContactCommandHandler(AppDbContext db) => _db = db;

    public async Task<int> Handle(CreateContactCommand r, CancellationToken ct)
    {
        var email = r.Email.Trim().ToLower();

        // Duplicate email check (per tenant — query filter already scopes to current tenant)
        if (await _db.Contacts.AnyAsync(c => c.Email.ToLower() == email, ct))
            throw new DuplicateEmailException($"A contact with email '{email}' already exists in this tenant.");

        var entity = new Contact
        {
            FirstName = r.FirstName.Trim(),
            LastName  = r.LastName.Trim(),
            Email     = email,
            Phone     = r.Phone?.Trim(),
            Company   = r.Company?.Trim(),
            JobTitle  = r.JobTitle?.Trim(),
            Notes     = r.Notes?.Trim()
        };
        _db.Contacts.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }
}
