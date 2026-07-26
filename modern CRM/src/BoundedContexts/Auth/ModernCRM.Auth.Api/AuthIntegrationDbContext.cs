using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ModernCRM.Auth.Api.UserSync;

public sealed class AuthIntegrationDbContext(DbContextOptions<AuthIntegrationDbContext> options) : DbContext(options)
{
    public DbSet<SyncedAuthUser> Users => Set<SyncedAuthUser>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.Entity<SyncedAuthUser>(e =>
        {
            e.ToTable("ModernAuthUsers");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => new { x.TenantId, x.Username }).IsUnique();
            e.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
            e.Property(x => x.TenantId).HasMaxLength(100).IsRequired();
            e.Property(x => x.Username).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
        });
    }
}
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