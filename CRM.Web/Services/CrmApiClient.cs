using System.Net.Http.Headers;
using System.Net.Http.Json;
using CRM.Web.Auth;

namespace CRM.Web.Services;

/// <summary>
/// Talks to the CRM API instance whose URL was provided in the login response.
/// The URL comes from Tenant → ServiceInstance.ApiUrl, resolved by AuthService.
/// </summary>
public class CrmApiClient
{
    private readonly IHttpClientFactory _factory;
    private readonly AuthStateProvider  _auth;

    public CrmApiClient(IHttpClientFactory factory, AuthStateProvider auth)
    {
        _factory = factory;
        _auth    = auth;
    }

    private async Task<HttpClient> BuildAsync()
    {
        var token  = await _auth.GetTokenAsync();
        var apiUrl = await _auth.GetApiUrlAsync();

        // SafeGetAsync returns null when the circuit has disconnected or during
        // prerender — treat that as a clean cancellation so Blazor disposes quietly.
        if (apiUrl is null)
            throw new OperationCanceledException("Circuit disconnected or not authenticated.");

        var http = _factory.CreateClient("crm");
        http.BaseAddress = new Uri(apiUrl);
        if (!string.IsNullOrWhiteSpace(token))
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return http;
    }

    // OperationCanceledException propagates upward — Blazor Server swallows it
    // silently when the component is already being torn down.

    public async Task<List<T>?> GetListAsync<T>(string path)
        => await (await BuildAsync()).GetFromJsonAsync<List<T>>(path);

    public async Task<T?> GetAsync<T>(string path)
        => await (await BuildAsync()).GetFromJsonAsync<T>(path);

    public async Task<HttpResponseMessage> PostAsync<T>(string path, T body)
        => await (await BuildAsync()).PostAsJsonAsync(path, body);

    public async Task<HttpResponseMessage> PutAsync<T>(string path, T body)
        => await (await BuildAsync()).PutAsJsonAsync(path, body);

    public async Task<HttpResponseMessage> DeleteAsync(string path)
        => await (await BuildAsync()).DeleteAsync(path);
}
