namespace ModernCRM.Crm.Application.DTO;

public sealed record AccountDto(int Id, string TenantId, string Name, string? Industry, string? Website, string? Phone, string? Address);
public sealed record ContactDto(int Id, string TenantId, string FirstName, string LastName, string Email, string? Phone, string? JobTitle, int? AccountId);
public sealed record TicketDto(int Id, string TenantId, string Subject, string? Description, int AccountId, int? ContactId, int? AssignedToAuthUserId, string Status, string Priority, DateTime? DueDate);
public sealed record OpportunityDto(int Id, string TenantId, string Title, decimal Value, int? AccountId, int? ContactId, string Stage, DateTime? ExpectedCloseDate);