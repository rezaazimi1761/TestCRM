using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.Contacts;

public sealed record ContactCreatedDomainEvent(int ContactId, string TenantId, string Email) : DomainEvent;
