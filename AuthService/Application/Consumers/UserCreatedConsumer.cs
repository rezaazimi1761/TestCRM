using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Events;

namespace AuthService.Application.Consumers;

/// <summary>
/// Consumes <see cref="UserCreatedEvent"/> published by TestCRM and creates
/// the corresponding auth account in AuthService's own database.
///
/// Inbox pattern guarantees idempotency: if the message is delivered more than
/// once (e.g. after a crash + retry), the second attempt is a no-op because
/// MassTransit records the MessageId in the InboxState table and skips
/// reprocessing for already-handled messages.
///
/// The consumer also handles the race-condition case where the username or
/// email already exists (previous partial success) by logging a warning and
/// completing successfully rather than throwing — preventing infinite retries.
/// </summary>
public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{
    private readonly AuthDbContext _db;
    private readonly ILogger<UserCreatedConsumer> _logger;

    public UserCreatedConsumer(AuthDbContext db, ILogger<UserCreatedConsumer> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var evt = context.Message;

        // Verify the tenant exists in AuthService
        var tenant = await _db.Tenants
            .FirstOrDefaultAsync(t => t.Slug == evt.TenantId && t.IsActive);

        if (tenant is null)
        {
            // Tenant missing — likely a timing issue during seeding.
            // Throw so MassTransit retries; eventually the tenant will exist.
            throw new InvalidOperationException(
                $"[UserCreatedConsumer] Tenant '{evt.TenantId}' not found or inactive. " +
                "Will retry.");
        }

        // Idempotency guard — if auth user was already created (e.g. on retry
        // after the consumer committed but the ack was lost), skip silently.
        var exists = await _db.Users.AnyAsync(
            u => u.TenantId == evt.TenantId &&
                 (u.Username == evt.Username || u.Email == evt.Email));

        if (exists)
        {
            _logger.LogWarning(
                "[UserCreatedConsumer] Auth user for tenant={TenantId} username={Username} " +
                "already exists — duplicate delivery, skipping.",
                evt.TenantId, evt.Username);
            return;
        }

        var user = new AppUser
        {
            TenantId     = evt.TenantId,
            Username     = evt.Username,
            Email        = evt.Email,
            FirstName    = evt.FirstName,
            LastName     = evt.LastName,
            PasswordHash = evt.PasswordHash,   // BCrypt hash from TestCRM — reusable
            Role         = evt.Role,
            IsActive     = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "[UserCreatedConsumer] Auth user created: tenant={TenantId} username={Username} role={Role}",
            evt.TenantId, evt.Username, evt.Role);
    }
}
