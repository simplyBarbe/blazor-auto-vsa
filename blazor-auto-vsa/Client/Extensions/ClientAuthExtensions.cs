using Client.Infrastructure.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Client.Extensions;

/// <summary>
/// Extension methods for configuring client-side authentication.
/// </summary>
public static class ClientAuthExtensions
{
    /// <summary>
    /// Adds authentication services to the client application.
    /// </summary>
    /// <param name="builder">The WebAssembly host builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static WebAssemblyHostBuilder AddClientAuthentication(this WebAssemblyHostBuilder builder)
    {
        // Register authorization services
        builder.Services.AddAuthorizationCore();

        // Register the authentication state provider
        builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthStateProvider>();
        builder.Services.AddScoped<CookieAuthStateProvider>(sp => (CookieAuthStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());

        return builder;
    }
}
