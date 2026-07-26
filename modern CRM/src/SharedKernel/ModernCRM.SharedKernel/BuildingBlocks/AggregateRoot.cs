namespace ModernCRM.SharedKernel.BuildingBlocks;

public abstract class AggregateRoot<TId> : Entity<TId>
{
    public long Version { get; protected set; }
    protected void Touch() => Version++;
}
