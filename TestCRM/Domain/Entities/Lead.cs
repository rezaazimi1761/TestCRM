using TestCRM.Domain.Common;

namespace TestCRM.Domain.Entities;

public enum LeadStatus { New, Contacted, Qualified, Lost, Converted }

public class Lead : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Source { get; set; }
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public string? Notes { get; set; }
    public int? AssignedToUserId { get; set; }
}
