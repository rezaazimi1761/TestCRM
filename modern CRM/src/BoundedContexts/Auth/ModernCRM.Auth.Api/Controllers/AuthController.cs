using Microsoft.AspNetCore.Mvc;

namespace ModernCRM.Auth.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Username and password are required." });

        return Ok(new AuthResponse(
            AccessToken: "demo-token",
            RefreshToken: "demo-refresh-token",
            ExpiresAt: DateTime.UtcNow.AddHours(8),
            UserId: 1,
            Username: request.Username,
            Role: "SuperUser",
            TenantId: request.TenantId,
            ServiceInstanceId: Guid.NewGuid(),
            ApiUrl: "http://localhost:5046"));
    }

    public sealed record LoginRequest(string TenantId, string Username, string Password);

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
}