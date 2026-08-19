using MassTransit;
using Microsoft.EntityFrameworkCore;
using ModernCRM.Auth.Application.Integration;
using ModernCRM.Auth.Infrastructure.Integration;

namespace ModernCRM.Auth.Infrastructure.Integration;

public sealed class AuthIntegrationDbContext(DbContextOptions<AuthIntegrationDbContext> options) : DbContext(options)
{
    public DbSet<SyncedAuthUser> Users => Set<SyncedAuthUser>();
    public DbSet<ServiceInstanceModel> ServiceInstances => Set<ServiceInstanceModel>();
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
        modelBuilder.Entity<ServiceInstanceModel>(e =>
        {
            e.ToTable("ModernAuthServiceInstances");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.ApiUrl).HasMaxLength(500).IsRequired();
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Ignore(x => x.TenantCount);
        });
    }
}
