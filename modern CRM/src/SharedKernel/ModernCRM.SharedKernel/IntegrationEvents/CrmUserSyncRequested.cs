namespace ModernCRM.SharedKernel.IntegrationEvents;

public sealed record CrmUserSyncRequested(Guid CorrelationId, UserIntegrationOperation Operation, int CrmUserId, int? AuthUserId, string TenantId, string Username, string Email, string FirstName, string LastName, string Role, bool IsActive, string? Password);
