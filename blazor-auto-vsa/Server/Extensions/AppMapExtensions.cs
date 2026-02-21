using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Server.Components;

namespace Server.Extensions;

/// <summary>
/// Maps OpenAPI, API endpoints, and Razor components in one place.
/// </summary>
public static class AppMapExtensions
{
    /// <summary>
    /// Maps OpenAPI documentation, API endpoints from the given assembly, and Razor components (Server + WebAssembly).
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <param name="endpointsAssembly">Assembly to scan for API endpoints.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication MapApp(this WebApplication app, Assembly endpointsAssembly)
    {
        app.MapOpenApiDocumentation();
        app.MapEndpoints(endpointsAssembly);
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        return app;
    }
}
