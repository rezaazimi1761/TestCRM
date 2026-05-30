using Shared.Domain.Common;

namespace TestCRM.Domain.Entities;

public enum ActivityType { Call, Email, Meeting, Task, Note }

public class Activity : BaseEntity
{
    public string Subject { get; set; } = string.Empty;
    public ActivityType Type { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }

    public int? ContactId { get; set; }
    public Contact? Contact { get; set; }

    public int? AssignedToUserId { get; set; }
}
