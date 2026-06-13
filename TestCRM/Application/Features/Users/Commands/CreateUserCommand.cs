using System.ComponentModel.DataAnnotations;
using MediatR;

namespace TestCRM.Application.Features.Users.Commands;

/// <summary>
/// User creation is owned by AuthService. This record is kept only for legacy request binding.
/// </summary>
public record CreateUserCommand(
    [Required(AllowEmptyStrings = false)] [StringLength(100)] string Username,
    [Required(AllowEmptyStrings = false)] [StringLength(100)] string FirstName,
    [Required(AllowEmptyStrings = false)] [StringLength(100)] string LastName,
    [Required(AllowEmptyStrings = false)] [EmailAddress] [StringLength(255)] string Email,
    [Required(AllowEmptyStrings = false)] [StringLength(128, MinimumLength = 6, ErrorMessage = "Password must be 6-128 characters.")] string Password,
    string Role = "User"
) : IRequest<int>;
