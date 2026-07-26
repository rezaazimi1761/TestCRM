namespace ModernCRM.SharedKernel.BuildingBlocks;

public abstract record DomainEvent(Guid EventId, DateTime OccurredAtUtc)
{
    protected DomainEvent() : this(Guid.NewGuid(), DateTime.UtcNow) { }
}
