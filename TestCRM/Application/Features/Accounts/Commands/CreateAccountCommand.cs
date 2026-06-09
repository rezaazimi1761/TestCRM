using System.ComponentModel.DataAnnotations;
using MediatR;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Accounts.Commands;

public record CreateAccountCommand(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required.")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Name must be 1–255 characters.")]
    string Name,
    [StringLength(100)] string? Industry,
    [StringLength(500)] string? Website,
    [StringLength(50)]  string? Phone,
    [StringLength(500)] string? Address,
    [StringLength(2000)] string? Notes
) : IRequest<int>;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, int>
{
    private readonly AppDbContext _db;
    public CreateAccountCommandHandler(AppDbContext db) => _db = db;

    public async Task<int> Handle(CreateAccountCommand r, CancellationToken ct)
    {
        var trimmedName = r.Name.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
            throw new ValidationException("Name cannot be empty or whitespace.");

        var entity = new Account
        {
            Name     = trimmedName,
            Industry = r.Industry?.Trim(),
            Website  = r.Website?.Trim(),
            Phone    = r.Phone?.Trim(),
            Address  = r.Address?.Trim(),
            Notes    = r.Notes?.Trim()
        };
        _db.Accounts.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }
}
