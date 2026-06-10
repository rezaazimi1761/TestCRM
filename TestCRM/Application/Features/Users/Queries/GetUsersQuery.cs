using TestCRM.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Users.Queries;

/// <summary>Safe projection — never exposes PasswordHash, TenantId, or IsDeleted.</summary>
public record UserDto(
    int Id, string Username, string FirstName, string LastName,
    string Email, string Role, bool IsActive,
    string AuthSyncStatus,
    DateTime CreatedAt, DateTime? UpdatedAt);

public record GetUsersQuery(
    int Page = 1, int PageSize = 20,
    string? SortBy = null, bool SortDesc = false,
    string? Search = null,
    string? Role = null
) : IRequest<PagedResult<UserDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly AppDbContext _db;
    public GetUsersQueryHandler(AppDbContext db) => _db = db;

    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery r, CancellationToken ct)
    {
        var pageSize = Math.Min(r.PageSize, PaginationValidator.MaxPageSize);

        var q = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(r.Search))
        {
            var s = r.Search.Trim().ToLower();
            q = q.Where(u =>
                u.FirstName.ToLower().Contains(s) ||
                u.LastName.ToLower().Contains(s)  ||
                u.Email.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(r.Role))
            q = q.Where(u => u.Role == r.Role);

        var total = await q.CountAsync(ct);

        q = r.SortBy switch
        {
            "lastname"  => r.SortDesc ? q.OrderByDescending(u => u.LastName)  : q.OrderBy(u => u.LastName),
            "email"     => r.SortDesc ? q.OrderByDescending(u => u.Email)     : q.OrderBy(u => u.Email),
            "role"      => r.SortDesc ? q.OrderByDescending(u => u.Role)      : q.OrderBy(u => u.Role),
            "isactive"  => r.SortDesc ? q.OrderByDescending(u => u.IsActive)  : q.OrderBy(u => u.IsActive),
            _           => r.SortDesc ? q.OrderByDescending(u => u.FirstName) : q.OrderBy(u => u.FirstName),
        };

        var items = await q
            .Skip((r.Page - 1) * pageSize).Take(pageSize)
            .Select(u => new UserDto(u.Id, u.Username, u.FirstName, u.LastName, u.Email, u.Role, u.IsActive, u.AuthSyncStatus, u.CreatedAt, u.UpdatedAt))
            .ToListAsync(ct);

        return new PagedResult<UserDto>(items, total, r.Page, pageSize);
    }
}
