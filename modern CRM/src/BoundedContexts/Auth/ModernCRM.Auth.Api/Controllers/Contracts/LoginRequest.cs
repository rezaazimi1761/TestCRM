using System.ComponentModel.DataAnnotations;
namespace ModernCRM.Auth.Api.Controllers;
public sealed record LoginRequest(
    [Required, StringLength(100)] string TenantId,
    [Required, StringLength(100)] string Username,
    [Required, StringLength(128)] string Password);
