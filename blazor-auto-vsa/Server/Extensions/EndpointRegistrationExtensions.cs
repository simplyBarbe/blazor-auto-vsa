using Server.Infrastructure.Auth;
using Server.Infrastructure.Endpoints;
using System.Reflection;

namespace Server.Extensions;

/// <summary>
/// Extension methods for registering API endpoints.
/// </summary>
public static class EndpointRegistrationExtensions
{
    /// <summary>
    /// Maps all API endpoints for the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    /// <param name="assembly">The assembly to scan for endpoints.</param>
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app, Assembly assembly)
    {
        // Map authentication endpoints
        app.MapAuthEndpoints();

        var endpointTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IEndpoint).IsAssignableFrom(t));

        foreach (var endpointType in endpointTypes)
        {
            var endpoint = (IEndpoint)Activator.CreateInstance(endpointType)!;
            endpoint.Map(app);
        }

        return app;
    }
}
