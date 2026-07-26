using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.Accounts;

public sealed record AccountRenamedDomainEvent(int AccountId, string Name) : DomainEvent;
