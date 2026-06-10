namespace Shared.Contracts.Events;

/// <summary>
/// Published by TestCRM (via Outbox) when a new user is created.
/// Consumed by AuthService (via Inbox) to create the corresponding auth account.
/// The password is already BCrypt-hashed — never send plaintext passwords on the bus.
/// </summary>
public record UserCreatedEvent(
    Guid   CorrelationId,
    string TenantId,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string PasswordHash,
    string Role
);
