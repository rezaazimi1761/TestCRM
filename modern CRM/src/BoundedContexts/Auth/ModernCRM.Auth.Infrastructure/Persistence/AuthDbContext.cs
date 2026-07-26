using ModernCRM.Auth.Domain.Tenants;
using ModernCRM.Auth.Domain.Users;

namespace ModernCRM.Auth.Infrastructure.Persistence;

public sealed class AuthDbContext
{
    private int _userIdSequence = 1;
    private int _tenantIdSequence = 1;

    public List<AuthUser> Users { get; } = new();
    public List<Tenant> Tenants { get; } = new();
    public List<object> OutboxMessages { get; } = new();
    public List<object> InboxMessages { get; } = new();

    public int NextUserId() => _userIdSequence++;
    public int NextTenantId() => _tenantIdSequence++;
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}
