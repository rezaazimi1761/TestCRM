using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.Opportunities;

public sealed record OpportunityStageChangedDomainEvent(int OpportunityId, OpportunityStage Stage) : DomainEvent;
