namespace ModernCRM.Web.Services;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    int UserId,
    string Username,
    string Role,
    string TenantId,
    Guid ServiceInstanceId,
    string ApiUrl);