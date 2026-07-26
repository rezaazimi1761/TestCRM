using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.Contacts;

public sealed record ContactCreatedDomainEvent(int ContactId, string TenantId, string Email) : DomainEvent;
public sealed record ContactAssignedToAccountDomainEvent(int ContactId, int AccountId) : DomainEvent;
public sealed record ContactDeletedDomainEvent(int ContactId) : DomainEvent;
