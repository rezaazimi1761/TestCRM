using TestCRM.Domain.Common;

namespace TestCRM.Domain.Entities;

public enum OpportunityStage { Prospecting, Qualification, Proposal, Negotiation, ClosedWon, ClosedLost }

public class Opportunity : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public OpportunityStage Stage { get; set; } = OpportunityStage.Prospecting;
    public DateTime? ExpectedCloseDate { get; set; }
    public string? Notes { get; set; }

    public int? ContactId { get; set; }
    public Contact? Contact { get; set; }

    public int? AccountId { get; set; }
    public Account? Account { get; set; }

    public int? AssignedToUserId { get; set; }
}
