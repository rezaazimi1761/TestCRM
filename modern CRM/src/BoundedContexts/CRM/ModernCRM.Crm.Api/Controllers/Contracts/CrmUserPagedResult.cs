namespace ModernCRM.Crm.Api.Controllers;

public sealed record CrmUserPagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
