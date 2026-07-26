namespace ModernCRM.SharedKernel.BuildingBlocks;

public abstract class Entity<TId>
{
    private readonly List<DomainEvent> _domainEvents = new();

    public TId Id { get; protected set; } = default!;
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void Raise(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
