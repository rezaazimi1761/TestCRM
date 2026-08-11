using MassTransit;
using Microsoft.EntityFrameworkCore;
using ModernCRM.Crm.Api.Frontend;

namespace ModernCRM.Crm.Api.UserSync;

public sealed class CrmIntegrationDbContext(DbContextOptions<CrmIntegrationDbContext> options) : DbContext(options)
{
    public DbSet<CrmUser> Users => Set<CrmUser>();
    public DbSet<UserSyncSagaState> UserSyncSagas => Set<UserSyncSagaState>();
    public DbSet<AccountModel> Accounts => Set<AccountModel>();
    public DbSet<ContactModel> Contacts => Set<ContactModel>();
    public DbSet<LeadModel> Leads => Set<LeadModel>();
    public DbSet<OpportunityModel> Opportunities => Set<OpportunityModel>();
    public DbSet<TicketModel> Tickets => Set<TicketModel>();
    public DbSet<ActivityModel> Activities => Set<ActivityModel>();

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
        ConfigureFrontend<AccountModel>(modelBuilder, "ModernCrmAccounts");
        ConfigureFrontend<ContactModel>(modelBuilder, "ModernCrmContacts");
        ConfigureFrontend<LeadModel>(modelBuilder, "ModernCrmLeads");
        ConfigureFrontend<OpportunityModel>(modelBuilder, "ModernCrmOpportunities");
        ConfigureFrontend<TicketModel>(modelBuilder, "ModernCrmTickets");
        ConfigureFrontend<ActivityModel>(modelBuilder, "ModernCrmActivities");
        modelBuilder.Entity<OpportunityModel>().Property(x => x.Value).HasPrecision(18, 2);
    }

    private static void ConfigureFrontend<TEntity>(ModelBuilder modelBuilder, string table) where TEntity : TenantModel
    {
        var entity = modelBuilder.Entity<TEntity>();
        entity.ToTable(table);
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedOnAdd();
        entity.Property(x => x.TenantId).HasMaxLength(100).IsRequired();
        entity.HasIndex(x => new { x.TenantId, x.IsDeleted });
    }
}
