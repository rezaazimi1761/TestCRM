using ModernCRM.Crm.Application.DTO;
using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Queries;

public sealed record GetAccountsQuery(string TenantId, string? Search = null) : IQuery<IReadOnlyList<AccountDto>>;
