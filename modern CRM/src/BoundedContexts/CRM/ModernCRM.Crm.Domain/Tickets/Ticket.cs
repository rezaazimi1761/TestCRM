using ModernCRM.SharedKernel.BuildingBlocks;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Crm.Domain.Tickets;

public enum TicketStatus { New, Active, Closed, Removed }
public enum TicketPriority { Low, Medium, High, Critical }

public sealed class Ticket : AggregateRoot<int>
{
    public TenantId TenantId { get; private set; } = null!;
    public int RequestedByAccountId { get; private set; }
    public int? ContactId { get; private set; }
    public int? AssignedToAuthUserId { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TicketStatus Status { get; private set; } = TicketStatus.New;
    public TicketPriority Priority { get; private set; } = TicketPriority.Medium;
    public DateTime? DueDate { get; private set; }
    public DateTime? ClosedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }

    private Ticket() { }

    public static Ticket Create(TenantId tenantId, int requestedByAccountId, string subject, TicketPriority priority, DateTime? dueDate)
    {
        Guard.Against(requestedByAccountId <= 0, "Ticket must be requested by a valid account.");
        Guard.Against(dueDate.HasValue && dueDate.Value.Date < DateTime.UtcNow.Date, "Due date cannot be in the past.");
        var ticket = new Ticket { TenantId = tenantId, RequestedByAccountId = requestedByAccountId, Priority = priority, DueDate = dueDate };
        ticket.ChangeSubject(subject);
        ticket.Raise(new TicketCreatedDomainEvent(ticket.Id, tenantId.Value, ticket.Subject));
        return ticket;
    }

    public void ChangeSubject(string subject) { EnsureNotFinal(); Subject = Guard.NotBlank(subject, nameof(Subject), 255); Touch(); }
    public void ChangeDescription(string? description) { EnsureNotFinal(); Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(); Touch(); }
    public void AssignToUser(int authUserId) { EnsureNotFinal(); Guard.Against(authUserId <= 0, "User id is invalid."); AssignedToAuthUserId = authUserId; Touch(); }
    public void LinkContact(int contactId) { EnsureNotFinal(); Guard.Against(contactId <= 0, "Contact id is invalid."); ContactId = contactId; Touch(); }

    public void ChangePriority(TicketPriority priority)
    {
        EnsureNotFinal();
        Priority = priority;
        Touch();
        Raise(new TicketPriorityChangedDomainEvent(Id, priority));
    }

    public void Activate()
    {
        Guard.Against(Status != TicketStatus.New, "Only new tickets can be activated.");
        Status = TicketStatus.Active;
        Touch();
        Raise(new TicketActivatedDomainEvent(Id));
    }

    public void Close()
    {
        Guard.Against(Status is TicketStatus.Closed or TicketStatus.Removed, "Ticket is already final.");
        Status = TicketStatus.Closed;
        ClosedAtUtc = DateTime.UtcNow;
        Touch();
        Raise(new TicketClosedDomainEvent(Id));
    }

    public void Remove()
    {
        if (Status == TicketStatus.Removed) return;
        Status = TicketStatus.Removed;
        IsDeleted = true;
        Touch();
        Raise(new TicketRemovedDomainEvent(Id));
    }

    private void EnsureNotFinal() => Guard.Against(Status is TicketStatus.Closed or TicketStatus.Removed, "Final ticket cannot be changed.");
}
