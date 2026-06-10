using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Events;

namespace AuthService.Application.Consumers;

/// <summary>
/// Consumes UserCreatedEvent (published by TestCRM) and creates the auth account.
///
/// Retry policy (configured on receive endpoint in Program.cs):
///   3 attempts — 5s / 15s / 30s intervals.
///   Each attempt rolls back fully on failure (Inbox transaction).
///   After all retries fail → MassTransit auto-publishes Fault&lt;UserCreatedEvent&gt;
///   → TestCRM FaultConsumer receives it → soft-deletes the CRM user.
///
/// On success → publishes UserAuthSyncedEvent
///   → TestCRM SyncedConsumer receives it → marks AuthSyncStatus = "Synced".
/// </summary>
public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{
    private readonly AuthDbContext               _db;
    private readonly ILogger<UserCreatedConsumer> _logger;

    public UserCreatedConsumer(AuthDbContext db, ILogger<UserCreatedConsumer> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var evt = context.Message;
        var ct  = context.CancellationToken;

        // ── Tenant check ────────────────────────────────────────────
        // Throw → MassTransit retries → after all retries Fault is published
        // → TestCRM compensates by soft-deleting the pending user.
        var tenant = await _db.Tenants
            .FirstOrDefaultAsync(t => t.Slug == evt.TenantId && t.IsActive, ct);

        if (tenant is null)
            throw new InvalidOperationException(
                $"Tenant '{evt.TenantId}' not found or inactive.");

        // ── Idempotency guard ───────────────────────────────────────
        // Duplicate delivery (ack lost after commit): skip insert, still publish Synced.
        var exists = await _db.Users.AnyAsync(
            u => u.TenantId == evt.TenantId &&
                 (u.Username == evt.Username || u.Email == evt.Email), ct);

        if (exists)
        {
            _logger.LogWarning(
                "[UserCreatedConsumer] Duplicate delivery — auth user already exists. " +
                "tenant={TenantId} username={Username}",
                evt.TenantId, evt.Username);

            // Publish Synced so TestCRM converges to correct state on retries.
            await context.Publish(new UserAuthSyncedEvent(
                evt.CorrelationId, evt.TenantId, evt.Username), ct);
            return;
        }

        // ── Create auth account ─────────────────────────────────────
        _db.Users.Add(new AppUser
        {
            TenantId     = evt.TenantId,
            Username     = evt.Username,
            Email        = evt.Email,
            FirstName    = evt.FirstName,
            LastName     = evt.LastName,
            PasswordHash = evt.PasswordHash,   // BCrypt hash — valid in both services
            Role         = evt.Role,
            IsActive     = true
        });

        // SaveChanges + InboxState committed in ONE transaction (UseEntityFrameworkOutbox).
        // If this throws → full rollback → MassTransit retries.
        await _db.SaveChangesAsync(ct);

        // ── Notify TestCRM of success ────────────────────────────────
        // This publish goes through AuthService's own outbox tables,
        // so it is also delivered reliably even if RabbitMQ is momentarily down.
        await context.Publish(new UserAuthSyncedEvent(
            evt.CorrelationId, evt.TenantId, evt.Username), ct);

        _logger.LogInformation(
            "[UserCreatedConsumer] Auth user created. tenant={TenantId} username={Username}",
            evt.TenantId, evt.Username);
    }
}
