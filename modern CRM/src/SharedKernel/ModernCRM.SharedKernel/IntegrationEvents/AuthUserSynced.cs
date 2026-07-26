namespace ModernCRM.SharedKernel.IntegrationEvents;

public sealed record AuthUserSynced(Guid CorrelationId, UserIntegrationOperation Operation, int CrmUserId, int AuthUserId, string TenantId, DateTime SyncedAtUtc);
