using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Contacts.Queries;

public record GetContactByIdQuery(int Id) : IRequest<ContactDto?>;

public class GetContactByIdQueryHandler : IRequestHandler<GetContactByIdQuery, ContactDto?>
{
    private readonly AppDbContext _db;
    public GetContactByIdQueryHandler(AppDbContext db) => _db = db;

    public Task<ContactDto?> Handle(GetContactByIdQuery request, CancellationToken ct)
        => _db.Contacts
            .Where(c => c.Id == request.Id)
            .Select(c => new ContactDto(c.Id, c.FirstName, c.LastName, c.Email, c.Phone, c.Company, c.JobTitle, c.Notes, c.CreatedAt, c.UpdatedAt))
            .FirstOrDefaultAsync(ct);
}
