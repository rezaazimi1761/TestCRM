using ModernCRM.Crm.Application.DTO;
using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Queries;

public sealed record GetContactsQuery(string TenantId, string? Search = null) : IQuery<IReadOnlyList<ContactDto>>;
