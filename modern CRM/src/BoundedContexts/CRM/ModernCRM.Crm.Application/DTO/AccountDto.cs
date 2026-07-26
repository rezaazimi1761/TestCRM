namespace ModernCRM.Crm.Application.DTO;

public sealed record AccountDto(int Id, string TenantId, string Name, string? Industry, string? Website, string? Phone, string? Address);
