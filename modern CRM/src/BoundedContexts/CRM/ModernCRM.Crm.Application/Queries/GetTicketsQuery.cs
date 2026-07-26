using ModernCRM.Crm.Application.DTO;
using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Queries;

public sealed record GetTicketsQuery(string TenantId, string? Status = null, string? Priority = null) : IQuery<IReadOnlyList<TicketDto>>;
