namespace Shared.Domain.Common;

/// <summary>
/// Base for every domain entity in the system.
/// The TenantId column is the multi-tenancy discriminator filtered via EF Global Query Filters.
/// </summary>
public abstract class BaseEntity
{
    public int      Id        { get; set; }
    public string   TenantId  { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool     IsDeleted { get; set; }
}
