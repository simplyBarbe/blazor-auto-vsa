using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Server.Components;

namespace Server.Extensions;

public static class AppMapExtensions
{
    public static WebApplication MapApp(this WebApplication app, Assembly endpointsAssembly)
    {
        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.MapStaticAssets().AllowAnonymous();
        }

        app.MapOpenApiDocumentation();
        app.MapEndpoints(endpointsAssembly);
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        return app;
    }
}
