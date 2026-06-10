namespace Shared.Contracts.Events;

/// <summary>
/// Published by AuthService after successfully creating the auth account.
/// TestCRM consumes this to mark AppUser.AuthSyncStatus = "Synced".
/// </summary>
public record UserAuthSyncedEvent(
    Guid   CorrelationId,
    string TenantId,
    string Username
);
