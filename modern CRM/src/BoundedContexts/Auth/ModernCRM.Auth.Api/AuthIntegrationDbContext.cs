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
