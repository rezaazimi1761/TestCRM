using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.Opportunities;

public sealed record OpportunityCreatedDomainEvent(int OpportunityId, string TenantId, string Title) : DomainEvent;
