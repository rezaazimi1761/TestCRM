using Microsoft.Extensions.Configuration;
using ModernCRM.Auth.Application.Handlers;
using ModernCRM.Auth.Domain.Roles;
using ModernCRM.Auth.Domain.Tenants;
using ModernCRM.Auth.Domain.Users;
using ModernCRM.Auth.Domain.ValueObjects;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Auth.Infrastructure.Persistence;

public static class AuthDataSeeder
{
    public static async Task SeedAsync(AuthDbContext db, IPasswordHasher hasher, IConfiguration configuration, CancellationToken ct = default)
    {
        var tenantSlug = configuration["Seed:DefaultTenantSlug"] ?? "default";
        if (db.Tenants.Count == 0)
        {
            var serviceInstanceId = configuration.GetValue<Guid>("Seed:DefaultServiceInstanceId");
            if (serviceInstanceId == Guid.Empty) throw new InvalidOperationException("Seed:DefaultServiceInstanceId must be configured.");
            var tenant = Tenant.Create(TenantId.Create(tenantSlug), configuration["Seed:DefaultTenantName"] ?? "Default Tenant", serviceInstanceId);
            tenant.GetType().GetProperty("Id")!.SetValue(tenant, db.NextTenantId());
            db.Tenants.Add(tenant);
        }
        if (db.Users.Count == 0)
        {
            var user = AuthUser.Register(TenantId.Create(tenantSlug), Username.Create(configuration["Seed:AdminUsername"] ?? "admin"), Email.Create(configuration["Seed:AdminEmail"] ?? "admin@local"), configuration["Seed:AdminFirstName"] ?? "Master", configuration["Seed:AdminLastName"] ?? "Admin", PasswordHash.FromHash(hasher.Hash(configuration["Seed:AdminPassword"] ?? "Admin@123")), Role.SuperUser);
            user.GetType().GetProperty("Id")!.SetValue(user, db.NextUserId());
            db.Users.Add(user);
        }
        await db.SaveChangesAsync(ct);
    }
}