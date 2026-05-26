using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Contacts.Queries;

public record GetContactsQuery : IRequest<List<Contact>>;

public class GetContactsQueryHandler : IRequestHandler<GetContactsQuery, List<Contact>>
{
    private readonly AppDbContext _db;
    public GetContactsQueryHandler(AppDbContext db) => _db = db;

    public Task<List<Contact>> Handle(GetContactsQuery request, CancellationToken ct)
        => _db.Contacts.ToListAsync(ct);
}
