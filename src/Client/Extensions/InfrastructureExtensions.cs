using Client.Dispatching;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core;
using System.Reflection;

namespace Client.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddSingleton<IRequestEndpointMapper, RequestEndpointMapper>();
        services.AddScoped<IRequestSender, HttpRequestSender>();

        foreach (var assembly in assemblies)
        {
            services.AddValidatorsFromAssembly(assembly);

            var routeDefinitionTypes = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IRouteDefinition).IsAssignableFrom(t));

            foreach (var type in routeDefinitionTypes)
            {
                services.AddSingleton(typeof(IRouteDefinition), type);
            }
        }

        return services;
    }
}
