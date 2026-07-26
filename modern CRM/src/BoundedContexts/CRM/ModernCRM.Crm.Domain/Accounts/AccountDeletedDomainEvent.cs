using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.Accounts;

public sealed record AccountDeletedDomainEvent(int AccountId) : DomainEvent;
