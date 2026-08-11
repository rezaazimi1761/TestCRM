using System.ComponentModel.DataAnnotations;
namespace ModernCRM.Auth.Api.Controllers;
public sealed record UpdateAuthUserRequest(
    [Required, EmailAddress, StringLength(200)] string? Email,
    [Required, StringLength(100)] string? FirstName,
    [Required, StringLength(100)] string? LastName,
    [Required, RegularExpression("^(User|Admin|SuperUser)$")] string? Role,
    bool IsActive);
