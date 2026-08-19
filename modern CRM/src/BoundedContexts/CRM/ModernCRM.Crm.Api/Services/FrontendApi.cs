namespace ModernCRM.Crm.Api.Frontend;

using System.Reflection;
using ModernCRM.Crm.Application.Frontend;

public static class FrontendApi
{
    public static string Tenant(System.Security.Claims.ClaimsPrincipal user)
        => user.FindFirst("tenant_id")?.Value
           ?? throw new UnauthorizedAccessException("The authenticated token does not contain a tenant_id claim.");

    public static PagedResult<T> Page<T>(IReadOnlyList<T> source, int page, int pageSize, string? sortBy, bool sortDesc)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize < 1 ? 20 : pageSize, 1, 500);
        var property = string.IsNullOrWhiteSpace(sortBy) ? typeof(T).GetProperty("Id") : typeof(T).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        IEnumerable<T> ordered = source;
        if (property is not null)
            ordered = sortDesc ? source.OrderByDescending(x => property.GetValue(x)) : source.OrderBy(x => property.GetValue(x));
        return new PagedResult<T>(ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList(), source.Count, page, pageSize);
    }

}
