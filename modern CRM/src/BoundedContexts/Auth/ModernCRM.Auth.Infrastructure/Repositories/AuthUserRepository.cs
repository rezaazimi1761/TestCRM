using ModernCRM.Auth.Domain.Users;
using ModernCRM.Auth.Domain.ValueObjects;
using ModernCRM.Auth.Infrastructure.Persistence;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Auth.Infrastructure.Repositories;

public sealed class AuthUserRepository : IAuthUserRepository
{
    private readonly AuthDbContext _db;
    public AuthUserRepository(AuthDbContext db) => _db = db;

    public Task AddAsync(AuthUser user, CancellationToken ct)
    {
        if (user.Id == 0) user.GetType().GetProperty("Id")!.SetValue(user, _db.NextUserId());
        _db.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByEmailAsync(TenantId tenantId, string email, CancellationToken ct)
        => Task.FromResult(_db.Users.Any(u => u.TenantId == tenantId && u.Email.Value == email.Trim().ToLowerInvariant() && !u.IsDeleted));

    public Task<AuthUser?> GetByIdAsync(int id, CancellationToken ct)
        => Task.FromResult(_db.Users.FirstOrDefault(u => u.Id == id));

    public Task<AuthUser?> GetByUsernameAsync(TenantId tenantId, Username username, CancellationToken ct)
        => Task.FromResult(_db.Users.FirstOrDefault(u => u.TenantId == tenantId && u.Username == username && !u.IsDeleted));

    public Task<IReadOnlyList<AuthUser>> ListByTenantAsync(TenantId tenantId, string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = _db.Users.Where(u => u.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            query = query.Where(u => u.Username.Value.Contains(s) || u.FirstName.ToLowerInvariant().Contains(s) || u.LastName.ToLowerInvariant().Contains(s) || u.Email.Value.Contains(s));
        }

        return Task.FromResult<IReadOnlyList<AuthUser>>(query.Skip((page - 1) * pageSize).Take(pageSize).ToList());
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
