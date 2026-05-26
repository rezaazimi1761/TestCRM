using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Accounts.Queries;

public record GetAccountByIdQuery(int Id) : IRequest<Account?>;

public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, Account?>
{
    private readonly AppDbContext _db;
    public GetAccountByIdQueryHandler(AppDbContext db) => _db = db;

    public Task<Account?> Handle(GetAccountByIdQuery request, CancellationToken ct)
        => _db.Accounts.FirstOrDefaultAsync(a => a.Id == request.Id, ct);
}
