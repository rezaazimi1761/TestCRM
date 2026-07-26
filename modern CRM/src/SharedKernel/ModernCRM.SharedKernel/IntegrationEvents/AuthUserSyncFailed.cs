namespace ModernCRM.SharedKernel.IntegrationEvents;

public sealed record AuthUserSyncFailed(Guid CorrelationId, int CrmUserId, string TenantId, string Error, DateTime FailedAtUtc);
