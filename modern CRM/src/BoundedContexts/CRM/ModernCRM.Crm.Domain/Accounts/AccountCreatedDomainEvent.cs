using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.Accounts;

public sealed record AccountCreatedDomainEvent(int AccountId, string TenantId, string Name) : DomainEvent;
