using System.ComponentModel.DataAnnotations;
namespace ModernCRM.Auth.Api.Controllers;
public sealed record CreateAuthUserRequest(
    [Required, RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$"), StringLength(100)] string? TenantId,
    [Required, RegularExpression("^[a-zA-Z0-9][a-zA-Z0-9._-]*$"), StringLength(100, MinimumLength=3)] string? Username,
    [Required, EmailAddress, StringLength(200)] string? Email,
    [Required, StringLength(100)] string? FirstName,
    [Required, StringLength(100)] string? LastName,
    [Required, StringLength(128, MinimumLength=8), RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z0-9]).+$", ErrorMessage="Password must include uppercase, lowercase, number and special character.")] string? Password,
    [Required, RegularExpression("^(User|Admin|SuperUser)$")] string? Role);
