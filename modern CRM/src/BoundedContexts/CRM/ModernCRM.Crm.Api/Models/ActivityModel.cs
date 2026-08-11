using System.Reflection;

namespace ModernCRM.Crm.Api.Frontend;

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
