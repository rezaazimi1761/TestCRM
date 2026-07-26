namespace ModernCRM.SharedKernel.IntegrationEvents;

public enum UserIntegrationOperation { Created, Updated, Deleted }
public sealed record CrmUserSyncRequested(Guid CorrelationId, UserIntegrationOperation Operation, int CrmUserId, int? AuthUserId, string TenantId, string Username, string Email, string FirstName, string LastName, string Role, bool IsActive, string? Password);
public sealed record SyncUserToAuth(Guid CorrelationId, UserIntegrationOperation Operation, int CrmUserId, int? AuthUserId, string TenantId, string Username, string Email, string FirstName, string LastName, string Role, bool IsActive, string? Password);
public sealed record AuthUserSynced(Guid CorrelationId, UserIntegrationOperation Operation, int CrmUserId, int AuthUserId, string TenantId, DateTime SyncedAtUtc);
public sealed record AuthUserSyncFailed(Guid CorrelationId, int CrmUserId, string TenantId, string Error, DateTime FailedAtUtc);