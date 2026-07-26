using ModernCRM.Crm.Domain.ValueObjects;
using ModernCRM.SharedKernel.BuildingBlocks;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Crm.Domain.Opportunities;

public sealed class Opportunity : AggregateRoot<int>
{
    public TenantId TenantId { get; private set; } = null!;
    public int? AccountId { get; private set; }
    public int? ContactId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public Money Value { get; private set; } = Money.Create(0);
    public OpportunityStage Stage { get; private set; } = OpportunityStage.Prospecting;
    public DateTime? ExpectedCloseDate { get; private set; }
    public bool IsDeleted { get; private set; }

    private Opportunity() { }

    public static Opportunity Create(TenantId tenantId, string title, Money value, int? accountId = null, DateTime? expectedCloseDate = null)
    {
        var opportunity = new Opportunity { TenantId = tenantId, Value = value, AccountId = accountId, ExpectedCloseDate = expectedCloseDate };
        opportunity.Rename(title);
        opportunity.Raise(new OpportunityCreatedDomainEvent(opportunity.Id, tenantId.Value, opportunity.Title));
        return opportunity;
    }

    public void Rename(string title) { EnsureOpen(); Title = Guard.NotBlank(title, nameof(Title), 255); Touch(); }
    public void ChangeValue(Money value) { EnsureOpen(); Value = value; Touch(); }
    public void LinkContact(int contactId) { EnsureOpen(); Guard.Against(contactId <= 0, "Contact id is invalid."); ContactId = contactId; Touch(); }

    public void MoveTo(OpportunityStage stage)
    {
        EnsureOpen();
        Guard.Against(Stage == OpportunityStage.Prospecting && stage == OpportunityStage.Negotiation, "Opportunity must be qualified before negotiation.");
        Stage = stage;
        Touch();
        Raise(new OpportunityStageChangedDomainEvent(Id, stage));
        if (stage == OpportunityStage.ClosedWon) Raise(new OpportunityWonDomainEvent(Id));
        if (stage == OpportunityStage.ClosedLost) Raise(new OpportunityLostDomainEvent(Id));
    }

    public void Delete() { if (IsDeleted) return; IsDeleted = true; Touch(); }
    private void EnsureOpen() => Guard.Against(IsDeleted || Stage is OpportunityStage.ClosedWon or OpportunityStage.ClosedLost, "Closed opportunity cannot be changed.");
}
