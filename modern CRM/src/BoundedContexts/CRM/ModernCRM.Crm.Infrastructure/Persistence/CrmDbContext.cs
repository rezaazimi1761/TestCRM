using Microsoft.EntityFrameworkCore;
using ModernCRM.Crm.Domain.Accounts;
using ModernCRM.Crm.Domain.Contacts;
using ModernCRM.Crm.Domain.Opportunities;
using ModernCRM.Crm.Domain.Tickets;
using ModernCRM.Crm.Domain.Users;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Crm.Infrastructure.Persistence;

public sealed class CrmDbContext(DbContextOptions<CrmDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<CrmUserReference> UserReferences => Set<CrmUserReference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("crm");
        ConfigureAccount(modelBuilder);
        ConfigureContact(modelBuilder);
        ConfigureTicket(modelBuilder);
        ConfigureOpportunity(modelBuilder);
        ConfigureUserReference(modelBuilder);
    }

    private static void ConfigureAccount(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Account>();
        ConfigureBase(entity, "Accounts");
        ConfigureTenant(entity);
        entity.Property(x => x.Name).HasMaxLength(255).IsRequired();
        entity.Ignore(x => x.ContactIds);
    }

    private static void ConfigureContact(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Contact>();
        ConfigureBase(entity, "Contacts");
        ConfigureTenant(entity);
        entity.Property(x => x.Email).HasConversion(x => x.Value, x => Email.Create(x)).HasMaxLength(200).IsRequired();
        entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
    }

    private static void ConfigureTicket(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Ticket>();
        ConfigureBase(entity, "Tickets");
        ConfigureTenant(entity);
        entity.Property(x => x.Subject).HasMaxLength(255).IsRequired();
    }

    private static void ConfigureOpportunity(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Opportunity>();
        ConfigureBase(entity, "Opportunities");
        ConfigureTenant(entity);
        entity.Property(x => x.Value)
            .HasConversion(x => x.Amount, x => ModernCRM.Crm.Domain.ValueObjects.Money.Create(x, "USD"))
            .HasPrecision(18, 2)
            .HasColumnName("Value");
    }

    private static void ConfigureUserReference(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<CrmUserReference>();
        ConfigureBase(entity, "UserReferences");
        ConfigureTenant(entity);
        entity.Property(x => x.Email).HasConversion(x => x.Value, x => Email.Create(x)).HasMaxLength(200).IsRequired();
        entity.HasIndex(x => new { x.TenantId, x.AuthUserId }).IsUnique();
    }

    private static void ConfigureBase<TEntity>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entity, string table)
        where TEntity : class
    {
        entity.ToTable(table);
        entity.HasKey("Id");
        entity.Property<int>("Id").ValueGeneratedOnAdd();
        entity.Ignore("DomainEvents");
    }

    private static void ConfigureTenant<TEntity>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entity)
        where TEntity : class
        => entity.Property<TenantId>("TenantId").HasConversion(x => x.Value, x => TenantId.Create(x)).HasMaxLength(100).IsRequired();
}
