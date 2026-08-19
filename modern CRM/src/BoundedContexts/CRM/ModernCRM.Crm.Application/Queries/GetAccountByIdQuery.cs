using ModernCRM.Crm.Application.DTO;
using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Queries;

public sealed record GetAccountByIdQuery(string TenantId, int Id) : IQuery<AccountDto?>;
