using System.Net.Http.Json;

namespace ModernCRM.Web.Services;

public class AuthApiClient
{
    private readonly HttpClient _http;
    public AuthApiClient(HttpClient http) => _http = http;

    public async Task<AuthResponse?> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        var res = await _http.PostAsJsonAsync("/api/auth/login", req, ct);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: ct);
    }
}
