using ModernCRM.Crm.Application.DTO;
using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Queries;

public sealed record GetAccountsQuery(string TenantId, string? Search = null) : IQuery<IReadOnlyList<AccountDto>>;
public sealed record GetAccountByIdQuery(int Id) : IQuery<AccountDto?>;

public sealed record GetContactsQuery(string TenantId, string? Search = null) : IQuery<IReadOnlyList<ContactDto>>;
public sealed record GetContactByIdQuery(int Id) : IQuery<ContactDto?>;

public sealed record GetTicketsQuery(string TenantId, string? Status = null, string? Priority = null) : IQuery<IReadOnlyList<TicketDto>>;
public sealed record GetTicketByIdQuery(int Id) : IQuery<TicketDto?>;

public sealed record GetOpportunitiesQuery(string TenantId, string? Stage = null, string? Search = null) : IQuery<IReadOnlyList<OpportunityDto>>;
public sealed record GetOpportunityByIdQuery(int Id) : IQuery<OpportunityDto?>;