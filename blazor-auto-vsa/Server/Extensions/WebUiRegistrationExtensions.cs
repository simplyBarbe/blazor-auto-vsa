using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using Server.Components;

namespace Server.Extensions;

/// <summary>
/// Registers web UI and cross-cutting HTTP services (Razor, Fluent UI, localization, antiforgery, HttpClient).
/// </summary>
public static class WebUiRegistrationExtensions
{
    /// <summary>
    /// Adds Razor components, Fluent UI, localization, HttpContextAccessor, antiforgery, and HttpClient registration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWebUiServices(this IServiceCollection services)
    {
        var razorComponents = services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();
        razorComponents.AddAuthenticationStateSerialization();

        services.AddFluentUIComponents();
        services.AddLocalization();

        services.AddHttpContextAccessor();
        services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
        services.AddHttpClient();
        // Scoped HttpClient for Blazor WASM components that inject HttpClient directly.
        services.AddScoped(_ => new HttpClient());

        return services;
    }
}
