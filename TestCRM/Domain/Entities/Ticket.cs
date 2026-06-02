using Shared.Domain.Common;

namespace TestCRM.Domain.Entities;

public enum TicketStatus   { New, Active, Closed, Removed }
public enum TicketPriority { Low, Medium, High, Critical }

public class Ticket : BaseEntity
{
    // ── Core ─────────────────────────────────────────────────────
    public string          Subject     { get; set; } = string.Empty;
    public string?         Description { get; set; }
    public TicketStatus    Status      { get; set; } = TicketStatus.New;
    public TicketPriority  Priority    { get; set; } = TicketPriority.Medium;

    // ── Relationships ─────────────────────────────────────────────
    public int?  AccountId        { get; set; }
    public Account? Account       { get; set; }

    public int?  ContactId        { get; set; }
    public Contact? Contact       { get; set; }

    public int?  AssignedToUserId { get; set; }

    // ── Dates ─────────────────────────────────────────────────────
    public DateTime? DueDate      { get; set; }
    public DateTime? ResolvedAt   { get; set; }

    // ── Extra ─────────────────────────────────────────────────────
    public string?   Category     { get; set; }
    public string?   Notes        { get; set; }
}
