using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.Opportunities;

public sealed record OpportunityCreatedDomainEvent(int OpportunityId, string TenantId, string Title) : DomainEvent;
public sealed record OpportunityStageChangedDomainEvent(int OpportunityId, OpportunityStage Stage) : DomainEvent;
public sealed record OpportunityWonDomainEvent(int OpportunityId) : DomainEvent;
public sealed record OpportunityLostDomainEvent(int OpportunityId) : DomainEvent;
