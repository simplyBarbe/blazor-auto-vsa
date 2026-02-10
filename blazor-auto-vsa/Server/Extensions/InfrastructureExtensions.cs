using FluentValidation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Server.Infrastructure.Dispatching;
using Server.Infrastructure.Common;
using Server.Infrastructure.Services;
using Shared.Core;
using System.Reflection;

namespace Server.Extensions;

/// <summary>
/// Extension methods for registering server infrastructure components.
/// </summary>
public static class InfrastructureExtensions
{
    /// <summary>
    /// Registers core server infrastructure: handlers, validators, and the request pipeline.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for components.</param>
    /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, params Assembly[] assemblies)
        {
            // Register Infrastructure Services
            services.AddHttpContextAccessor();
            services.AddScoped<IDateTimeProvider, DateTimeProvider>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // 1. Register request pipeline (ValidationBehavior, IAsyncRequestValidator)
        services.AddRequestPipeline();

        // 2. Register Dispatcher for SSR/Prerendering
        services.AddScoped<IRequestSender, LocalRequestSender>();

        // 3. Scan assemblies for handlers and validators
        foreach (var assembly in assemblies)
        {
            services.AddHandlersFromAssembly(assembly);
            services.AddValidatorsFromAssembly(assembly);
        }

        return services;
    }

    /// <summary>
    /// Maps all API endpoints from the specified assemblies.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    /// <param name="assemblies">The assemblies to scan for endpoints.</param>
    /// <returns>The endpoint route builder for chaining.</returns>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            app.MapApiEndpoints(assembly);
        }

        return app;
    }
}
