using Microsoft.EntityFrameworkCore;
using ModernCRM.Auth.Domain.Tenants;
using ModernCRM.Auth.Domain.Users;
using ModernCRM.Auth.Domain.ValueObjects;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Auth.Infrastructure.Persistence;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<AuthUser> Users => Set<AuthUser>();
    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.TenantId).HasConversion(x => x.Value, x => TenantId.Create(x)).HasMaxLength(100).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => x.TenantId).IsUnique();
            entity.Ignore(x => x.DomainEvents);
        });

        modelBuilder.Entity<AuthUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.TenantId).HasConversion(x => x.Value, x => TenantId.Create(x)).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Username).HasConversion(x => x.Value, x => Username.Create(x)).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasConversion(x => x.Value, x => Email.Create(x)).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PasswordHash).HasConversion(x => x.Value, x => PasswordHash.FromHash(x)).HasMaxLength(500).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.IntegrationStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.IntegrationError).HasMaxLength(2000);
            entity.HasIndex(x => new { x.TenantId, x.Username }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
            entity.Ignore(x => x.DomainEvents);
            entity.OwnsMany(x => x.Claims, claims =>
            {
                claims.ToTable("UserClaims");
                claims.WithOwner().HasForeignKey("AuthUserId");
                claims.HasKey(x => x.Id);
                claims.Property(x => x.Id).ValueGeneratedOnAdd();
                claims.Property(x => x.Type).HasMaxLength(200).IsRequired();
                claims.Property(x => x.Value).HasMaxLength(500).IsRequired();
                claims.Ignore(x => x.DomainEvents);
            });
        });
    }
}
