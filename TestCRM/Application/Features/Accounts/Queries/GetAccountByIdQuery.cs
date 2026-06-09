using MediatR;
using Microsoft.EntityFrameworkCore;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Accounts.Queries;

public record GetAccountByIdQuery(int Id) : IRequest<AccountDto?>;

public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, AccountDto?>
{
    private readonly AppDbContext _db;
    public GetAccountByIdQueryHandler(AppDbContext db) => _db = db;

    public Task<AccountDto?> Handle(GetAccountByIdQuery request, CancellationToken ct)
        => _db.Accounts
            .Where(a => a.Id == request.Id)
            .Select(a => new AccountDto(a.Id, a.Name, a.Industry, a.Website, a.Phone, a.Address, a.Notes, a.CreatedAt, a.UpdatedAt))
            .FirstOrDefaultAsync(ct);
}
