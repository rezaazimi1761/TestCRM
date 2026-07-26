using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ModernCRM.Auth.Api.UserSync;

public sealed class SyncedAuthUser
{
    public int Id { get; set; }
    public int CrmUserId { get; set; }
    public string TenantId { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
