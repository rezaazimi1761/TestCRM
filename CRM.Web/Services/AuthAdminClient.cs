using System.Net.Http.Headers;
using System.Net.Http.Json;
using CRM.Web.Auth;

namespace CRM.Web.Services;

/// <summary>
/// Authenticated client for AuthService admin endpoints (tenants, service instances, claims).
/// Attaches the current JWT on every call.
/// </summary>
public class AuthAdminClient
{
    private readonly IHttpClientFactory _factory;
    private readonly IConfiguration     _cfg;
    private readonly AuthStateProvider  _auth;

    public AuthAdminClient(IHttpClientFactory factory, IConfiguration cfg, AuthStateProvider auth)
    {
        _factory = factory;
        _cfg     = cfg;
        _auth    = auth;
    }

    private async Task<HttpClient> BuildAsync()
    {
        var http = _factory.CreateClient("auth-admin");
        http.BaseAddress = new Uri(_cfg["AuthService:Url"] ?? "http://localhost:9041");
        var token = await _auth.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return http;
    }

    public async Task<List<T>?> GetListAsync<T>(string path)
        => await (await BuildAsync()).GetFromJsonAsync<List<T>>(path);

    public async Task<PagedResult<T>?> GetPagedAsync<T>(
        string path, int page, int pageSize,
        string? sortBy = null, bool sortDesc = false,
        string? search = null,
        Dictionary<string, string?>? extraFilters = null)
    {
        var url = $"{path}?page={page}&pageSize={pageSize}&sortDesc={sortDesc}";
        if (!string.IsNullOrWhiteSpace(sortBy)) url += $"&sortBy={Uri.EscapeDataString(sortBy)}";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";
        if (extraFilters != null)
            foreach (var (key, value) in extraFilters)
                if (!string.IsNullOrWhiteSpace(value))
                    url += $"&{key}={Uri.EscapeDataString(value)}";
        return await (await BuildAsync()).GetFromJsonAsync<PagedResult<T>>(url);
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string path, T body)
        => await (await BuildAsync()).PostAsJsonAsync(path, body);

    public async Task<HttpResponseMessage> PutAsync<T>(string path, T body)
        => await (await BuildAsync()).PutAsJsonAsync(path, body);

    public async Task<HttpResponseMessage> DeleteAsync(string path)
        => await (await BuildAsync()).DeleteAsync(path);
}
