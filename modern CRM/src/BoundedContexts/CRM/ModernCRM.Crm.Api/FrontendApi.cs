using System.Reflection;

namespace ModernCRM.Crm.Api.Frontend;

public static class FrontendApi
{
    public static string Tenant(System.Security.Claims.ClaimsPrincipal user) => user.FindFirst("tenant_id")?.Value ?? "default";

    public static PagedResult<T> Page<T>(IEnumerable<T> source, int page, int pageSize, string? sortBy, bool sortDesc)
    {
        page = page < 1 ? 1 : page;
        pageSize = Math.Clamp(pageSize < 1 ? 20 : pageSize, 1, 500);
        var items = source.ToList();
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            var property = typeof(T).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property is not null)
                items = (sortDesc ? items.OrderByDescending(x => property.GetValue(x)?.ToString()) : items.OrderBy(x => property.GetValue(x)?.ToString())).ToList();
        }
        else items = items.OrderByDescending(x => typeof(T).GetProperty("Id")?.GetValue(x)).ToList();
        return new PagedResult<T>(items.Skip((page - 1) * pageSize).Take(pageSize).ToList(), items.Count, page, pageSize);
    }

    public static bool Contains(string? value, string search) => value?.Contains(search, StringComparison.OrdinalIgnoreCase) == true;
}
