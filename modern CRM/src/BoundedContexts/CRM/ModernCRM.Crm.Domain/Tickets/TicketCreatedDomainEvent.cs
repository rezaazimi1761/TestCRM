using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.Tickets;

public sealed record TicketCreatedDomainEvent(int TicketId, string TenantId, string Subject) : DomainEvent;
