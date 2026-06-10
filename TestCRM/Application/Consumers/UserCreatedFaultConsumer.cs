using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Events;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Consumers;

/// <summary>
/// Compensation consumer — received when AuthService exhausted all retries
/// and could NOT create the auth account.
///
/// MassTransit automatically publishes Fault&lt;UserCreatedEvent&gt; after the last retry fails.
///
/// Action: soft-delete the CRM user (IsDeleted = true) and mark AuthSyncStatus = "Failed"
/// so the admin knows manual intervention is required.
/// </summary>
public class UserCreatedFaultConsumer : IConsumer<Fault<UserCreatedEvent>>
{
    private readonly AppDbContext                     _db;
    private readonly ILogger<UserCreatedFaultConsumer> _logger;

    public UserCreatedFaultConsumer(AppDbContext db, ILogger<UserCreatedFaultConsumer> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<Fault<UserCreatedEvent>> context)
    {
        var original = context.Message.Message;   // the original UserCreatedEvent
        var ct       = context.CancellationToken;
        var error    = context.Message.Exceptions.FirstOrDefault()?.Message ?? "unknown";

        _logger.LogError(
            "[UserCreatedFaultConsumer] Auth account creation failed after all retries. " +
            "tenant={TenantId} username={Username} error={Error}",
            original.TenantId, original.Username, error);

        var user = await _db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                u => u.TenantId == original.TenantId && u.Username == original.Username, ct);

        if (user is null)
        {
            _logger.LogWarning(
                "[UserCreatedFaultConsumer] CRM user not found for fault. " +
                "tenant={TenantId} username={Username}",
                original.TenantId, original.Username);
            return;
        }

        // Compensate: soft-delete the CRM user so it does not appear in lists,
        // and mark Failed so admin can investigate / retry manually.
        user.IsDeleted      = true;
        user.AuthSyncStatus = AuthSync.Failed;
        user.UpdatedAt      = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogWarning(
            "[UserCreatedFaultConsumer] CRM user soft-deleted (compensation). " +
            "tenant={TenantId} username={Username}. Admin must recreate manually.",
            original.TenantId, original.Username);
    }
}
