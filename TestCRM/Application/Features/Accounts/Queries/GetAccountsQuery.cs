using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Accounts.Queries;

public record GetAccountsQuery : IRequest<List<Account>>;

public class GetAccountsQueryHandler : IRequestHandler<GetAccountsQuery, List<Account>>
{
    private readonly AppDbContext _db;
    public GetAccountsQueryHandler(AppDbContext db) => _db = db;

    public Task<List<Account>> Handle(GetAccountsQuery request, CancellationToken ct)
        => _db.Accounts.ToListAsync(ct);
}
