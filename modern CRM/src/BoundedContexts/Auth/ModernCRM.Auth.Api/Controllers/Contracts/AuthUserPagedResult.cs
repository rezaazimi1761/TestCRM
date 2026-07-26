namespace ModernCRM.Auth.Api.Controllers;

public sealed record AuthUserPagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
