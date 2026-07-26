namespace ModernCRM.Crm.Application.DTO;

public sealed record ContactDto(int Id, string TenantId, string FirstName, string LastName, string Email, string? Phone, string? JobTitle, int? AccountId);
