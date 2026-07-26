using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.Opportunities;

public sealed record OpportunityWonDomainEvent(int OpportunityId) : DomainEvent;
