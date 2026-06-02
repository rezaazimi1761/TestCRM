using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

namespace AuthService.Infrastructure.Persistence;

/// <summary>
/// On first start of AuthService, seeds:
///   1. A placeholder ServiceInstance (fixed GUID) so the default tenant has a valid FK.
///   2. A "default" tenant attached to that instance.
///   3. A master "admin" SuperUser inside the default tenant.
///
/// All credentials & names are overridable from configuration under "Seed".
/// Idempotent — re-running is a no-op.
/// </summary>
public static class DataSeeder
{
    // A well-known GUID for the placeholder instance — admins can edit later
    // and a real CRM service that self-registers will update its row.

    public static async Task SeedAsync(AuthDbContext db, IConfiguration cfg, ILogger logger,
                                       CancellationToken ct = default)
    {
        var defaultServiceInstanceId = Guid.Parse(cfg["Seed:DefaultServiceInstanceId"] ?? "d91c58cd-fc1c-493d-b037-31f146ecd1f3");
        var defaultInstanceUrl  = cfg["Seed:DefaultServiceInstanceUrl"] ?? "http://localhost:9040";
        var defaultInstanceName = cfg["Seed:DefaultServiceInstanceName"] ?? "default-instance";
        var defaultTenantSlug   = cfg["Seed:DefaultTenantSlug"]          ?? "default";
        var defaultTenantName   = cfg["Seed:DefaultTenantName"]          ?? "Default Tenant";
        var adminUsername       = cfg["Seed:AdminUsername"]              ?? "admin";
        var adminPassword       = cfg["Seed:AdminPassword"]              ?? "Admin@123";
        var adminEmail          = cfg["Seed:AdminEmail"]                 ?? "admin@local";
        var adminFirstName      = cfg["Seed:AdminFirstName"]             ?? "Master";
        var adminLastName       = cfg["Seed:AdminLastName"]              ?? "Admin";

        // 1. ServiceInstance ────────────────────────────────────────
        var instance = await db.ServiceInstances.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == defaultServiceInstanceId, ct);

        if (instance is null)
        {
            instance = new ServiceInstance
            {
                Id          = defaultServiceInstanceId,
                Name        = defaultInstanceName,
                ApiUrl      = defaultInstanceUrl,
                Description = "Placeholder instance created by AuthService seeder. " +
                              "Will be updated when a real CRM instance registers.",
                IsActive    = true
            };
            db.ServiceInstances.Add(instance);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded default ServiceInstance {Id} → {Url}",
                instance.Id, instance.ApiUrl);
        }

        // 2. Tenant ─────────────────────────────────────────────────
        var tenant = await db.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Slug == defaultTenantSlug, ct);

        if (tenant is null)
        {
            tenant = new Tenant
            {
                Slug              = defaultTenantSlug,
                DisplayName       = defaultTenantName,
                Description       = "Auto-created default tenant.",
                ServiceInstanceId = instance.Id,
                IsActive          = true
            };
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded default Tenant '{Slug}'", tenant.Slug);
        }

        // 3. Master admin (SuperUser) ───────────────────────────────
        var admin = await db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.TenantId == defaultTenantSlug
                                   && u.Username == adminUsername, ct);

        if (admin is null)
        {
            admin = new AppUser
            {
                TenantId     = defaultTenantSlug,
                Username     = adminUsername,
                Email        = adminEmail,
                FirstName    = adminFirstName,
                LastName     = adminLastName,
                PasswordHash = BC.HashPassword(adminPassword),
                Role         = "SuperUser",
                IsActive     = true
            };
            db.Users.Add(admin);
            await db.SaveChangesAsync(ct);

            logger.LogWarning(
                "Seeded master admin user '{User}' in tenant '{Tenant}' with role SuperUser. " +
                "⚠️  Change the default password '{Password}' immediately in production.",
                adminUsername, defaultTenantSlug, adminPassword);
        }
    }
}
