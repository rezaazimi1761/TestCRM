using System.Reflection;

namespace ModernCRM.Crm.Api.Frontend;

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
