using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.Tickets;

public sealed record TicketCreatedDomainEvent(int TicketId, string TenantId, string Subject) : DomainEvent;
public sealed record TicketActivatedDomainEvent(int TicketId) : DomainEvent;
public sealed record TicketClosedDomainEvent(int TicketId) : DomainEvent;
public sealed record TicketRemovedDomainEvent(int TicketId) : DomainEvent;
public sealed record TicketPriorityChangedDomainEvent(int TicketId, TicketPriority Priority) : DomainEvent;
