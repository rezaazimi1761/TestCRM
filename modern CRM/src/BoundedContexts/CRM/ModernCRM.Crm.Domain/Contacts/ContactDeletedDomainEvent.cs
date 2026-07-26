using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.Contacts;

public sealed record ContactDeletedDomainEvent(int ContactId) : DomainEvent;
