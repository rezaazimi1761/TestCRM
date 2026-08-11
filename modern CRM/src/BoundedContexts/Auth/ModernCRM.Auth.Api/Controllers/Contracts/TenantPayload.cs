using System.ComponentModel.DataAnnotations;
namespace ModernCRM.Auth.Api.Controllers;
public sealed record TenantPayload(
    [Required, RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$"), StringLength(100)] string? Slug,
    [Required, StringLength(200)] string? DisplayName,
    [Required] Guid ServiceInstanceId,
    [StringLength(2000)] string? Description,
    bool? IsActive);
