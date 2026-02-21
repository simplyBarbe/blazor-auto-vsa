using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Client.Extensions;

/// <summary>
/// Registers the API HttpClient with Antiforgery support for server communication.
/// </summary>
public static class ApiHttpClientExtensions
{
    /// <summary>
    /// Registers AntiforgeryHandler and the named "API" HttpClient with base address from the host environment.
    /// Also registers scoped HttpClient resolution via IHttpClientFactory for standard injection.
    /// </summary>
    /// <param name="builder">The WebAssembly host builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static WebAssemblyHostBuilder AddApiHttpClient(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddScoped<Client.Infrastructure.Auth.AntiforgeryHandler>();

        builder.Services.AddHttpClient("API", client =>
        {
            client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
        })
            .AddHttpMessageHandler<Client.Infrastructure.Auth.AntiforgeryHandler>();

        builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

        return builder;
    }
}
