using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Leads.Queries;

public record GetLeadByIdQuery(int Id) : IRequest<Lead?>;

public class GetLeadByIdQueryHandler : IRequestHandler<GetLeadByIdQuery, Lead?>
{
    private readonly AppDbContext _db;
    public GetLeadByIdQueryHandler(AppDbContext db) => _db = db;

    public Task<Lead?> Handle(GetLeadByIdQuery request, CancellationToken ct)
        => _db.Leads.FirstOrDefaultAsync(l => l.Id == request.Id, ct);
}
