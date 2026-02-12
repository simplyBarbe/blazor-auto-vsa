using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

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
        // Register authorization services.
        builder.Services.AddAuthorizationCore();
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddAuthenticationStateDeserialization();

        return builder;
    }
}
