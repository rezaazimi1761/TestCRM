using System.ComponentModel.DataAnnotations;
namespace ModernCRM.Auth.Api.Controllers;
public sealed record ServiceInstancePayload(
    [Required, StringLength(200)] string? Name,
    [Required, Url, StringLength(500)] string? ApiUrl,
    [StringLength(2000)] string? Description,
    bool? IsActive);
