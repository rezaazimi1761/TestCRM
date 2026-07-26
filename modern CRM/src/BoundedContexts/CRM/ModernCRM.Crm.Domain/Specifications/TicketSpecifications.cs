using System.Linq.Expressions;
using ModernCRM.Crm.Domain.Tickets;
using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.Specifications;

public sealed class OpenTicketsSpecification : Specification<Ticket>
{
    public override Expression<Func<Ticket, bool>> Criteria => ticket =>
        (ticket.Status == TicketStatus.New || ticket.Status == TicketStatus.Active) && !ticket.IsDeleted;
}
