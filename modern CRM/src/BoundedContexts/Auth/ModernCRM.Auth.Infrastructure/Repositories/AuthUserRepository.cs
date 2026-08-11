using ModernCRM.Auth.Domain.Users;
using ModernCRM.Auth.Domain.ValueObjects;
using ModernCRM.Auth.Infrastructure.Persistence;
using ModernCRM.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ModernCRM.Auth.Infrastructure.Repositories;

public sealed class AuthUserRepository : IAuthUserRepository
{
    private readonly AuthDbContext _db;
    public AuthUserRepository(AuthDbContext db) => _db = db;

    public async Task AddAsync(AuthUser user, CancellationToken ct) => await _db.Users.AddAsync(user, ct);

    public Task<bool> ExistsByEmailAsync(TenantId tenantId, string email, CancellationToken ct)
        => _db.Users.AnyAsync(u => u.TenantId == tenantId && u.Email == Email.Create(email) && !u.IsDeleted, ct);

    public Task<AuthUser?> GetByIdAsync(int id, CancellationToken ct)
        => _db.Users.Include(u => u.Claims).FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<AuthUser?> GetByUsernameAsync(TenantId tenantId, Username username, CancellationToken ct)
        => _db.Users.Include(u => u.Claims).FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Username == username && !u.IsDeleted, ct);

    public async Task<IReadOnlyList<AuthUser>> ListByTenantAsync(TenantId tenantId, string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = _db.Users.Where(u => u.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(u => u.FirstName.Contains(s) || u.LastName.Contains(s));
        }

        return await query.AsNoTracking().OrderBy(u => u.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
