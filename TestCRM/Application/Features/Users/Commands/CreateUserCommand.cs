using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Application.Common;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Users.Commands;

public record CreateUserCommand(
    [property: Required(AllowEmptyStrings = false)] [property: StringLength(100)] string FirstName,
    [property: Required(AllowEmptyStrings = false)] [property: StringLength(100)] string LastName,
    [property: Required(AllowEmptyStrings = false)] [property: EmailAddress] [property: StringLength(255)] string Email,
    [property: Required(AllowEmptyStrings = false)] [property: StringLength(128, MinimumLength = 6, ErrorMessage = "Password must be 6–128 characters.")] string Password,
    string Role = "User"
) : IRequest<int>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
{
    private readonly AppDbContext _db;

    public CreateUserCommandHandler(AppDbContext db) => _db = db;

    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLower();

        // Duplicate email check (per tenant — query filter already scopes to current tenant)
        if (await _db.Users.AnyAsync(u => u.Email.ToLower() == email, cancellationToken))
            throw new DuplicateEmailException($"A user with email '{email}' already exists in this tenant.");

        var user = new AppUser
        {
            FirstName    = request.FirstName.Trim(),
            LastName     = request.LastName.Trim(),
            Email        = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role         = request.Role
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}
