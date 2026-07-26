using ModernCRM.Crm.Application.DTO;
using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Queries;

public sealed record GetOpportunitiesQuery(string TenantId, string? Stage = null, string? Search = null) : IQuery<IReadOnlyList<OpportunityDto>>;
