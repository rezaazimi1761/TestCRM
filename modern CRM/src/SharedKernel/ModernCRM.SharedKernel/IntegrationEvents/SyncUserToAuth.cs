namespace ModernCRM.SharedKernel.IntegrationEvents;

public sealed record SyncUserToAuth(Guid CorrelationId, UserIntegrationOperation Operation, int CrmUserId, int? AuthUserId, string TenantId, string Username, string Email, string FirstName, string LastName, string Role, bool IsActive, string? Password);
