using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<Tenant>       Tenants       { get; set; }
    public DbSet<AppUser>      Users         { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<UserClaim>    UserClaims    { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ── Tenant ─────────────────────────────────────────────────
        mb.Entity<Tenant>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Slug       ).HasMaxLength(100).IsRequired();
            e.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);

            // Soft-delete filter
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        // ── AppUser ────────────────────────────────────────────────
        mb.Entity<AppUser>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Username }).IsUnique();
            e.HasIndex(x => new { x.TenantId, x.Email    }).IsUnique();
            e.Property(x => x.TenantId).HasMaxLength(100).IsRequired();
            e.Property(x => x.Username).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email   ).HasMaxLength(200).IsRequired();
            e.Property(x => x.Role    ).HasMaxLength(50 ).IsRequired();

            // FK  AppUser.TenantId → Tenant.Slug
            e.HasOne(x => x.Tenant)
             .WithMany(t => t.Users)
             .HasForeignKey(x => x.TenantId)
             .HasPrincipalKey(t => t.Slug)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);

            // Soft-delete filter
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        // ── RefreshToken ───────────────────────────────────────────
        mb.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(x => x.UserId)
             .IsRequired(false);
        });

        // ── UserClaim ──────────────────────────────────────────────
        mb.Entity<UserClaim>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User)
             .WithMany(u => u.Claims)
             .HasForeignKey(x => x.UserId)
             .IsRequired(false);
        });
    }
}
