using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ModernCRM.Auth.Domain.Users;

namespace ModernCRM.Auth.Infrastructure.Identity;

public interface IJwtTokenService
{
    string GenerateAccessToken(AuthUser user);
    string GenerateRefreshToken();
}

public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public string GenerateAccessToken(AuthUser user)
    {
        var secret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
        var expires = DateTime.UtcNow.AddMinutes(configuration.GetValue("Jwt:AccessTokenMinutes", 60));
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email.Value),
            new(ClaimTypes.Name, user.Username.Value),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("tenant_id", user.TenantId.Value),
            new("home_tenant_id", user.TenantId.Value),
            new("first_name", user.FirstName),
            new("last_name", user.LastName)
        };
        claims.AddRange(user.Claims.Select(c => new Claim(c.Type, c.Value)));
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"], audience: configuration["Jwt:Audience"], claims: claims, expires: expires,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)), SecurityAlgorithms.HmacSha256)));
    }

    public string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}