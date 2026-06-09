using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Leads.Queries;

public record GetLeadByIdQuery(int Id) : IRequest<LeadDto?>;

public class GetLeadByIdQueryHandler : IRequestHandler<GetLeadByIdQuery, LeadDto?>
{
    private readonly AppDbContext _db;
    public GetLeadByIdQueryHandler(AppDbContext db) => _db = db;

    public Task<LeadDto?> Handle(GetLeadByIdQuery request, CancellationToken ct)
        => _db.Leads
            .Where(l => l.Id == request.Id)
            .Select(l => new LeadDto(l.Id, l.FirstName, l.LastName, l.Email, l.Phone, l.Company, l.Source, l.Status, l.Notes, l.AssignedToUserId, l.CreatedAt, l.UpdatedAt))
            .FirstOrDefaultAsync(ct);
}
