using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.Tickets;

public sealed record TicketClosedDomainEvent(int TicketId) : DomainEvent;
