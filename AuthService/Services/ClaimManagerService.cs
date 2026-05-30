using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services;

public interface IClaimManagerService
{
    Task<List<UserClaim>> GetClaimsAsync(int userId, CancellationToken ct = default);
    Task AddClaimAsync   (int userId, string type, string value, CancellationToken ct = default);
    Task RemoveClaimAsync(int claimId, CancellationToken ct = default);
    Task ReplaceClaimsAsync(int userId, IEnumerable<(string type, string value)> claims, CancellationToken ct = default);
}

public class ClaimManagerService : IClaimManagerService
{
    private readonly AuthDbContext _db;
    public ClaimManagerService(AuthDbContext db) => _db = db;

    public Task<List<UserClaim>> GetClaimsAsync(int userId, CancellationToken ct)
        => _db.UserClaims.Where(c => c.UserId == userId).ToListAsync(ct);

    public async Task AddClaimAsync(int userId, string type, string value, CancellationToken ct)
    {
        _db.UserClaims.Add(new UserClaim { UserId = userId, ClaimType = type, ClaimValue = value });
        await _db.SaveChangesAsync(ct);
    }

    public async Task RemoveClaimAsync(int claimId, CancellationToken ct)
    {
        var claim = await _db.UserClaims.FindAsync(new object[] { claimId }, ct);
        if (claim is not null)
        {
            _db.UserClaims.Remove(claim);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task ReplaceClaimsAsync(int userId, IEnumerable<(string type, string value)> claims, CancellationToken ct)
    {
        var existing = await _db.UserClaims.Where(c => c.UserId == userId).ToListAsync(ct);
        _db.UserClaims.RemoveRange(existing);
        _db.UserClaims.AddRange(claims.Select(c => new UserClaim
        {
            UserId = userId, ClaimType = c.type, ClaimValue = c.value
        }));
        await _db.SaveChangesAsync(ct);
    }
}
