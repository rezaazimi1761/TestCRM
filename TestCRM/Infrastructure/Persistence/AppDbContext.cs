using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces;
using Shared.Domain.Common;
using TestCRM.Domain.Entities;

namespace TestCRM.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ITenantService _tenantService;
    private readonly string _currentTenant;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
        _currentTenant = _tenantService.GetCurrentTenant();
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── MassTransit Outbox tables (no tenant filter — infrastructure tables) ──
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(u => new { u.Email,    u.TenantId }).IsUnique();
            entity.HasIndex(u => new { u.Username, u.TenantId }).IsUnique();
            entity.HasQueryFilter(e => e.TenantId == _currentTenant && !e.IsDeleted);
        });
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasQueryFilter(e => e.TenantId == _currentTenant && !e.IsDeleted);
        });
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasQueryFilter(e => e.TenantId == _currentTenant && !e.IsDeleted);
        });
        modelBuilder.Entity<Lead>(entity =>
        {
            entity.HasQueryFilter(e => e.TenantId == _currentTenant && !e.IsDeleted);
        });
        modelBuilder.Entity<Opportunity>(entity =>
        {
            entity.HasQueryFilter(e => e.TenantId == _currentTenant && !e.IsDeleted);
        });
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasQueryFilter(e => e.TenantId == _currentTenant && !e.IsDeleted);
        });
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasQueryFilter(e => e.TenantId == _currentTenant && !e.IsDeleted);
        });
    }



    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.TenantId = _currentTenant;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
