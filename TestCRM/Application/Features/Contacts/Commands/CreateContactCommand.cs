using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Application.Common;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Contacts.Commands;

public record CreateContactCommand(
    [Required(AllowEmptyStrings = false)] [StringLength(100)] string FirstName,
    [Required(AllowEmptyStrings = false)] [StringLength(100)] string LastName,
    [Required(AllowEmptyStrings = false)] [EmailAddress] [StringLength(255)] string Email,
    [StringLength(50)]   string? Phone,
    [StringLength(255)]  string? Company,
    [StringLength(150)]  string? JobTitle,
    [StringLength(2000)] string? Notes
) : IRequest<int>;

public class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, int>
{
    private readonly AppDbContext _db;
    public CreateContactCommandHandler(AppDbContext db) => _db = db;

    public async Task<int> Handle(CreateContactCommand r, CancellationToken ct)
    {
        var email = r.Email.Trim().ToLower();

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
