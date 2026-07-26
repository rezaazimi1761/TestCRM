using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.Contacts;

public sealed record ContactAssignedToAccountDomainEvent(int ContactId, int AccountId) : DomainEvent;
