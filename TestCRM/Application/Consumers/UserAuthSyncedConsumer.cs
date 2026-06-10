using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Events;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Consumers;

/// <summary>
/// Received when AuthService successfully created the auth account.
/// Marks AppUser.AuthSyncStatus = "Synced".
/// Uses IgnoreQueryFilters() because this consumer runs outside an HTTP request
/// (no tenant claim in JWT) — we filter manually by TenantId.
/// </summary>
public class UserAuthSyncedConsumer : IConsumer<UserAuthSyncedEvent>
{
    private readonly AppDbContext                    _db;
    private readonly ILogger<UserAuthSyncedConsumer> _logger;

    public UserAuthSyncedConsumer(AppDbContext db, ILogger<UserAuthSyncedConsumer> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserAuthSyncedEvent> context)
    {
        var evt = context.Message;
        var ct  = context.CancellationToken;

        var user = await _db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                u => u.TenantId == evt.TenantId && u.Username == evt.Username, ct);

        if (user is null)
        {
            _logger.LogWarning(
                "[UserAuthSyncedConsumer] CRM user not found. tenant={TenantId} username={Username}",
                evt.TenantId, evt.Username);
            return; // idempotent — already deleted or never existed
        }

        if (user.AuthSyncStatus == AuthSync.Synced)
        {
            _logger.LogDebug(
                "[UserAuthSyncedConsumer] Already Synced — duplicate delivery, skipping.");
            return;
        }

        user.AuthSyncStatus = AuthSync.Synced;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "[UserAuthSyncedConsumer] AuthSyncStatus = Synced. tenant={TenantId} username={Username}",
            evt.TenantId, evt.Username);
    }
}
