using ModernCRM.Auth.Domain.Roles;
using ModernCRM.Auth.Domain.Users;
using ModernCRM.Auth.Domain.ValueObjects;
using ModernCRM.Crm.Domain.Accounts;
using ModernCRM.SharedKernel.ValueObjects;
using Xunit;

namespace ModernCRM.ArchitectureTests;

public sealed class DomainTests
{
    [Fact]
    public void Rehydrated_auth_user_preserves_identity_and_does_not_raise_events()
    {
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var user = AuthUser.Rehydrate(42, TenantId.Create("tenant-a"), Username.Create("alice"), Email.Create("alice@example.test"), "Alice", "Tester", PasswordHash.FromHash("hash"), Role.User, true, createdAt);

        Assert.Equal(42, user.Id);
        Assert.Equal("tenant-a", user.TenantId.Value);
        Assert.Equal(createdAt, user.CreatedAtUtc);
        Assert.Empty(user.DomainEvents);
    }

    [Fact]
    public void Account_keeps_its_tenant_boundary()
    {
        var account = Account.Create(TenantId.Create("tenant-a"), "Acme");
        Assert.Equal("tenant-a", account.TenantId.Value);
    }
}
