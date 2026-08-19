using ModernCRM.Auth.Domain.Users;

namespace ModernCRM.Auth.Application.Handlers;

public interface IJwtTokenService
{
    string GenerateAccessToken(AuthUser user);
    string GenerateRefreshToken();
}

