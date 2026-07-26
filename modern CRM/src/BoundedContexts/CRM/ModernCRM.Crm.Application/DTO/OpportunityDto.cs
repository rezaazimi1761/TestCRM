namespace ModernCRM.Crm.Application.DTO;

public sealed record OpportunityDto(int Id, string TenantId, string Title, decimal Value, int? AccountId, int? ContactId, string Stage, DateTime? ExpectedCloseDate);
