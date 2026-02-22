using Server.Infrastructure.Auth;
using Server.Infrastructure.Endpoints;
using System.Reflection;

namespace Server.Extensions;

public static class EndpointRegistrationExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app, Assembly assembly)
    {
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
