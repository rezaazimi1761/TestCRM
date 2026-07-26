using System.Reflection;

namespace ModernCRM.Crm.Api.Frontend;

public sealed class FrontendCrmStore
{
    private int _nextId;
    public object SyncRoot { get; } = new();
    public List<AccountModel> Accounts { get; } = new();
    public List<ContactModel> Contacts { get; } = new();
    public List<LeadModel> Leads { get; } = new();
    public List<OpportunityModel> Opportunities { get; } = new();
    public List<TicketModel> Tickets { get; } = new();
    public List<ActivityModel> Activities { get; } = new();
    public int NextId() => Interlocked.Increment(ref _nextId);
}

public abstract class TenantModel
{
    public int Id { get; set; }
    public string TenantId { get; set; } = "default";
    public bool IsDeleted { get; set; }
}

public sealed class AccountModel : TenantModel
{
    public string? Name { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
}
public sealed class ContactModel : TenantModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public string? Notes { get; set; }
    public int? AccountId { get; set; }
}
public sealed class LeadModel : TenantModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string Status { get; set; } = "New";
    public string? Source { get; set; }
    public string? Notes { get; set; }
}
public sealed class OpportunityModel : TenantModel
{
    public string? Title { get; set; }
    public decimal Value { get; set; }
    public string Stage { get; set; } = "Prospecting";
    public int? AccountId { get; set; }
    public string? AccountName { get; set; }
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public int? AssignedToUserId { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public string? Notes { get; set; }
}
public sealed class TicketModel : TenantModel
{
    public string Subject { get; set; } = "";
    public string? Description { get; set; }
    public string Status { get; set; } = "New";
    public string Priority { get; set; } = "Medium";
    public int? AccountId { get; set; }
    public string? AccountName { get; set; }
    public int? ContactId { get; set; }
    public int? AssignedToUserId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? Category { get; set; }
    public string? Notes { get; set; }
}
public sealed class ActivityModel : TenantModel
{
    public string? Subject { get; set; }
    public string Type { get; set; } = "Task";
    public string? Description { get; set; }
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}

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