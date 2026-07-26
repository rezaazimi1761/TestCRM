using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.Tickets;

public sealed record TicketPriorityChangedDomainEvent(int TicketId, TicketPriority Priority) : DomainEvent;
