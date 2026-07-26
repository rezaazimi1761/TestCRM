using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ModernCRM.Crm.Api.UserSync;

public sealed class CrmIntegrationDbContext(DbContextOptions<CrmIntegrationDbContext> options) : DbContext(options)
{
    public DbSet<CrmUser> Users => Set<CrmUser>();
    public DbSet<UserSyncSagaState> UserSyncSagas => Set<UserSyncSagaState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.Entity<CrmUser>(e =>
        {
            e.ToTable("ModernCrmUsers");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Username }).IsUnique();
            e.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
            e.Property(x => x.TenantId).HasMaxLength(100).IsRequired();
            e.Property(x => x.Username).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.SyncStatus).HasMaxLength(32).IsRequired();
            e.Property(x => x.SyncError).HasMaxLength(2000);
        });
        modelBuilder.Entity<UserSyncSagaState>(e =>
        {
            e.ToTable("ModernCrmUserSyncSagas");
            e.HasKey(x => x.CorrelationId);
            e.Property(x => x.CurrentState).HasMaxLength(64).IsRequired();
            e.Property(x => x.TenantId).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.CrmUserId);
        });
    }
}

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

public sealed class UserSyncSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "";
    public int CrmUserId { get; set; }
    public string TenantId { get; set; } = "";
    public string Operation { get; set; } = "";
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Error { get; set; }
}