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
