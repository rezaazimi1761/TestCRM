using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Contacts.Queries;

public record GetContactByIdQuery(int Id) : IRequest<Contact?>;

public class GetContactByIdQueryHandler : IRequestHandler<GetContactByIdQuery, Contact?>
{
    private readonly AppDbContext _db;
    public GetContactByIdQueryHandler(AppDbContext db) => _db = db;

    public Task<Contact?> Handle(GetContactByIdQuery request, CancellationToken ct)
        => _db.Contacts.FirstOrDefaultAsync(c => c.Id == request.Id, ct);
}
