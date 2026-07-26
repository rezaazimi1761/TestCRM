using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace ModernCRM.Web.Auth;

public class AuthStateProvider : AuthenticationStateProvider
{
    private const string TokenKey  = "crm_access_token";
    private const string TenantKey = "crm_tenant_id";
    private const string ApiUrlKey = "crm_api_url";

    private readonly ILocalStorageService _storage;
    public AuthStateProvider(ILocalStorageService storage) => _storage = storage;

    // Swallow JS-interop errors that occur when the circuit is gone (navigation /
    // tab close races with an ongoing async chain) or during server prerendering.
    private async Task<string?> SafeGetAsync(string key)
    {
        try   { return await _storage.GetItemAsStringAsync(key); }
        catch (JSDisconnectedException)  { return null; }
        catch (JSException)              { return null; }
        catch (InvalidOperationException){ return null; }
        catch (OperationCanceledException){ return null; }
    }

    public Task<string?> GetTokenAsync()  => SafeGetAsync(TokenKey);
    public Task<string?> GetTenantAsync() => SafeGetAsync(TenantKey);
    public Task<string?> GetApiUrlAsync() => SafeGetAsync(ApiUrlKey);

    public async Task SignInAsync(string token, string tenantId, string apiUrl)
    {
        await _storage.SetItemAsStringAsync(TokenKey,  token);
        await _storage.SetItemAsStringAsync(TenantKey, tenantId);
        await _storage.SetItemAsStringAsync(ApiUrlKey, apiUrl);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SignOutAsync()
    {
        await _storage.RemoveItemAsync(TokenKey);
        await _storage.RemoveItemAsync(TenantKey);
        await _storage.RemoveItemAsync(ApiUrlKey);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // SafeGetAsync already handles prerender / circuit-disconnected cases.
        var token = await GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token)) return Anonymous;

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            if (jwt.ValidTo < DateTime.UtcNow) return Anonymous;

            var identity = new ClaimsIdentity(jwt.Claims, "jwt", "unique_name", ClaimTypes.Role);
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return Anonymous;
        }
    }
}
