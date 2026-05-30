namespace AuthService.Domain.Entities;

public class Tenant
{
    public int    Id          { get; set; }

    /// <summary>Unique slug used as TenantId in every other table (e.g. "acme", "contoso").</summary>
    public string Slug        { get; set; } = default!;

    public string DisplayName { get; set; } = default!;
    public string? Description { get; set; }
    public bool   IsActive    { get; set; } = true;
    public bool   IsDeleted   { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}
