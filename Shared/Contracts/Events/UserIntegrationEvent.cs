namespace Shared.Contracts.Events;

public enum UserIntegrationOperation
{
    Created,
    Updated,
    Deleted
}

/// <summary>
/// Wrapper event for all AuthService -> CRM user projection changes.
/// AuthService owns users; CRM consumes this event to maintain a local projection.
/// </summary>
public record UserIntegrationEvent(
    Guid CorrelationId,
    UserIntegrationOperation Operation,
    int AuthUserId,
    string TenantId,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive);

/// <summary>
/// Sent by CRM after the user projection change is applied.
/// AuthService saga consumes this to close the integration workflow.
/// </summary>
public record UserIntegrationAppliedEvent(
    Guid CorrelationId,
    UserIntegrationOperation Operation,
    int AuthUserId,
    string TenantId,
    DateTime AppliedAt);
