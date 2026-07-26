using System.Reflection;

namespace ModernCRM.Crm.Api.Frontend;

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
