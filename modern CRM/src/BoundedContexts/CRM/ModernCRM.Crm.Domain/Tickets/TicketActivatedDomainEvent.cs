using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.Tickets;

public sealed record TicketActivatedDomainEvent(int TicketId) : DomainEvent;
