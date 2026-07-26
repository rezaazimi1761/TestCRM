namespace ModernCRM.Crm.Api.Controllers;

public sealed record CrmUserView(int Id, int? AuthUserId, string TenantId, string Username, string FirstName, string LastName, string Email, string Role, bool IsActive, bool IsDeleted, string IntegrationStatus, string? IntegrationError, DateTime CreatedAt);
