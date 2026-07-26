using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.Opportunities;

public sealed record OpportunityLostDomainEvent(int OpportunityId) : DomainEvent;
