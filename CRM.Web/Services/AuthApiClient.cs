using System.Net.Http.Json;

namespace CRM.Web.Services;

public class AuthApiClient
{
    private readonly HttpClient _http;
    public AuthApiClient(HttpClient http) => _http = http;

    public record LoginRequest(string TenantId, string Username, string Password);

    public record AuthResponse(
        string   AccessToken,
        string   RefreshToken,
        DateTime ExpiresAt,
        int      UserId,
        string   Username,
        string   Role,
        string   TenantId,
        Guid     ServiceInstanceId,
        string   ApiUrl);

    public async Task<AuthResponse?> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        var res = await _http.PostAsJsonAsync("/api/auth/login", req, ct);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: ct);
    }
}
