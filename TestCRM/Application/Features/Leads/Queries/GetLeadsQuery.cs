using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Leads.Queries;

public record GetLeadsQuery : IRequest<List<Lead>>;

public class GetLeadsQueryHandler : IRequestHandler<GetLeadsQuery, List<Lead>>
{
    private readonly AppDbContext _db;
    public GetLeadsQueryHandler(AppDbContext db) => _db = db;

    public Task<List<Lead>> Handle(GetLeadsQuery request, CancellationToken ct)
        => _db.Leads.ToListAsync(ct);
}
