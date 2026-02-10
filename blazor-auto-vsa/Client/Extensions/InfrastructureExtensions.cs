using Client.Dispatching;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core;
using System.Reflection;

namespace Client.Extensions;

/// <summary>
/// Extension methods for registering client infrastructure services.
/// </summary>
public static class InfrastructureExtensions
{
    /// <summary>
    /// Registers all client-side infrastructure services (dispatcher, validators, routes).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for components (validators, route definitions).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, params Assembly[] assemblies)
    {
        // 1. Register Dispatcher infrastructure
        services.AddSingleton<IRequestEndpointMapper, RequestEndpointMapper>();
        services.AddScoped<IRequestSender, HttpRequestSender>();

        // 2. Scan assemblies for validators and route definitions
        foreach (var assembly in assemblies)
        {
            // Register FluentValidation validators
            services.AddValidatorsFromAssembly(assembly);

            // Register all IRouteDefinition implementations for the RequestEndpointMapper
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
