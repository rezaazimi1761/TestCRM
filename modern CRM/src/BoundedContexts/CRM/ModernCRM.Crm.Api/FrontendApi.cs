using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ModernCRM.Crm.Api.Frontend;

public static class FrontendApi
{
    public static string Tenant(System.Security.Claims.ClaimsPrincipal user) => user.FindFirst("tenant_id")?.Value ?? "default";

    public static async Task<PagedResult<T>> PageAsync<T>(IQueryable<T> source, int page, int pageSize, string? sortBy, bool sortDesc, CancellationToken ct) where T : class
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize < 1 ? 20 : pageSize, 1, 500);
        var total = await source.CountAsync(ct);
        var property = string.IsNullOrWhiteSpace(sortBy)
            ? "Id"
            : typeof(T).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)?.Name ?? "Id";
        source = sortDesc
            ? source.OrderByDescending(x => EF.Property<object>(x!, property))
            : source.OrderBy(x => EF.Property<object>(x!, property));
        var items = await source.Skip((page - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync(ct);
        return new PagedResult<T>(items, total, page, pageSize);
    }
}
