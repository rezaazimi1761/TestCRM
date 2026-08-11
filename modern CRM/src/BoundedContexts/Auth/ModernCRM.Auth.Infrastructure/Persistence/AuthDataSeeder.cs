using Microsoft.Extensions.Configuration;
using ModernCRM.Auth.Application.Handlers;
using ModernCRM.Auth.Domain.Roles;
using ModernCRM.Auth.Domain.Tenants;
using ModernCRM.Auth.Domain.Users;
using ModernCRM.Auth.Domain.ValueObjects;
using ModernCRM.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ModernCRM.Auth.Infrastructure.Persistence;

public static class AuthDataSeeder
{
    public static async Task SeedAsync(AuthDbContext db, IPasswordHasher hasher, IConfiguration configuration, CancellationToken ct = default)
    {
        var tenantSlug = configuration["Seed:DefaultTenantSlug"] ?? "default";
        if (!await db.Tenants.AnyAsync(ct))
        {
            var serviceInstanceId = configuration.GetValue<Guid>("Seed:DefaultServiceInstanceId");
            if (serviceInstanceId == Guid.Empty) throw new InvalidOperationException("Seed:DefaultServiceInstanceId must be configured.");
            var tenant = Tenant.Create(TenantId.Create(tenantSlug), configuration["Seed:DefaultTenantName"] ?? "Default Tenant", serviceInstanceId);
            await db.Tenants.AddAsync(tenant, ct);
        }
        if (!await db.Users.AnyAsync(ct))
        {
            var user = AuthUser.Register(TenantId.Create(tenantSlug), Username.Create(configuration["Seed:AdminUsername"] ?? "admin"), Email.Create(configuration["Seed:AdminEmail"] ?? "admin@local"), configuration["Seed:AdminFirstName"] ?? "Master", configuration["Seed:AdminLastName"] ?? "Admin", PasswordHash.FromHash(hasher.Hash(configuration["Seed:AdminPassword"] ?? "Admin@123")), Role.SuperUser);
            await db.Users.AddAsync(user, ct);
        }
        await db.SaveChangesAsync(ct);
    }
}
