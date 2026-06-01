using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace CRM.Web.Auth;

public class AuthStateProvider : AuthenticationStateProvider
{
    private const string TokenKey  = "crm_access_token";
    private const string TenantKey = "crm_tenant_id";
    private const string ApiUrlKey = "crm_api_url";

    private readonly ILocalStorageService _storage;
    public AuthStateProvider(ILocalStorageService storage) => _storage = storage;

    public async Task<string?> GetTokenAsync()  => await _storage.GetItemAsStringAsync(TokenKey);
    public async Task<string?> GetTenantAsync() => await _storage.GetItemAsStringAsync(TenantKey);
    public async Task<string?> GetApiUrlAsync() => await _storage.GetItemAsStringAsync(ApiUrlKey);

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
        string? token;
        try
        {
            // During server prerendering JS interop is not yet available.
            // Treat that case as "not authenticated yet" instead of throwing.
            token = await GetTokenAsync();
        }
        catch (InvalidOperationException)
        {
            return Anonymous;
        }

        if (string.IsNullOrWhiteSpace(token)) return Anonymous;

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            if (jwt.ValidTo < DateTime.UtcNow) return Anonymous;

            var identity = new ClaimsIdentity(jwt.Claims, "jwt", "unique_name", "role");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return Anonymous;
        }
    }
}
