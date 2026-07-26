using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.Tickets;

public sealed record TicketRemovedDomainEvent(int TicketId) : DomainEvent;
