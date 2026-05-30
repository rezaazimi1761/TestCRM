using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services;

public interface IJwtTokenService
{
    string  GenerateAccessToken (AppUser user, IEnumerable<UserClaim> extraClaims);
    string  GenerateRefreshToken();
    ClaimsPrincipal? ValidatePrincipal(string token);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _cfg;
    public JwtTokenService(IConfiguration cfg) => _cfg = cfg;

    // ── Access Token ───────────────────────────────────────────────
    public string GenerateAccessToken(AppUser user, IEnumerable<UserClaim> extraClaims)
    {
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Secret"]!));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(_cfg["Jwt:AccessTokenMinutes"] ?? "60"));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new(ClaimTypes.Name,               user.Username),
            new(ClaimTypes.Role,               user.Role),
            new("tenant_id",                   user.TenantId),
            new("first_name",                  user.FirstName),
            new("last_name",                   user.LastName),
        };

        // attach custom DB claims
        claims.AddRange(extraClaims.Select(c => new Claim(c.ClaimType, c.ClaimValue)));

        var token = new JwtSecurityToken(
            issuer:             _cfg["Jwt:Issuer"],
            audience:           _cfg["Jwt:Audience"],
            claims:             claims,
            expires:            expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ── Refresh Token (opaque) ─────────────────────────────────────
    public string GenerateRefreshToken()
    {
        var rng = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(rng);
    }

    // ── Validate (used by gRPC ValidateToken) ─────────────────────
    public ClaimsPrincipal? ValidatePrincipal(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Secret"]!));
        var handler = new JwtSecurityTokenHandler();
        try
        {
            return handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer              = _cfg["Jwt:Issuer"],
                ValidAudience            = _cfg["Jwt:Audience"],
                IssuerSigningKey         = key,
                ClockSkew                = TimeSpan.Zero
            }, out _);
        }
        catch { return null; }
    }
}
