namespace ModernCRM.SharedKernel.IntegrationEvents;

public enum UserIntegrationOperation { Created, Updated, Deleted }

public sealed record UserIntegrationEvent(
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

public sealed record UserIntegrationAppliedEvent(
    Guid CorrelationId,
    UserIntegrationOperation Operation,
    int AuthUserId,
    string TenantId,
    DateTime AppliedAtUtc);
