namespace ModernCRM.Crm.Application.Frontend;

public sealed class CrmUser
{
    public int Id { get; set; }
    public int? AuthUserId { get; set; }
    public string TenantId { get; set; } = "default";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public string SyncStatus { get; set; } = "Pending";
    public string? SyncError { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
