using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Shared.Core.Auth;

namespace Client.Infrastructure.Auth;

/// <summary>
/// Custom authentication state provider that uses cookie-based authentication.
/// Fetches user information from the server using the authentication cookie.
/// </summary>
public class CookieAuthStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly PersistentComponentState _state;
    private UserInfo? _cachedUser;

    public CookieAuthStateProvider(HttpClient httpClient, PersistentComponentState state)
    {
        _httpClient = httpClient;
        _state = state;

        // Try to initialize from persisted state (prerendering)
        if (_state.TryTakeFromJson<UserInfo>(nameof(UserInfo), out var userInfo) && userInfo != null)
        {
            _cachedUser = userInfo;
        }
    }

    /// <summary>
    /// Gets the authentication state by fetching user info from the server.
    /// </summary>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_cachedUser != null)
        {
            return CreateAuthenticationState(_cachedUser);
        }

        try
        {
            // Create request with credentials included for WASM
            var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
            request.SetBrowserRequestOption("credentials", "include");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var userInfo = await response.Content.ReadFromJsonAsync<UserInfo>();
                if (userInfo != null)
                {
                    _cachedUser = userInfo;
                    return CreateAuthenticationState(userInfo);
                }
            }
        }
        catch
        {
            // If fetching user info fails, return anonymous
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    /// <summary>
    /// Notifies that the authentication state has changed after login.
    /// </summary>
    public void NotifyAuthenticationStateChanged(UserInfo? userInfo)
    {
        _cachedUser = userInfo;
        var authState = userInfo != null
            ? CreateAuthenticationState(userInfo)
            : new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

    /// <summary>
    /// Creates an authentication state from user information.
    /// </summary>
    private static AuthenticationState CreateAuthenticationState(UserInfo userInfo)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userInfo.UserId),
            new(ClaimTypes.Email, userInfo.Email),
            new(ClaimTypes.Name, userInfo.Email)
        };

        // Add roles
        claims.AddRange(userInfo.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Add additional claims
        claims.AddRange(userInfo.Claims.Select(c => new Claim(c.Key, c.Value)));

        var identity = new ClaimsIdentity(claims, "Cookie");
        var principal = new ClaimsPrincipal(identity);

        return new AuthenticationState(principal);
    }
}
