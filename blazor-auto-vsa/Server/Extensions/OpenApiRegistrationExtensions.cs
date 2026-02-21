using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace Server.Extensions;

/// <summary>
/// Registers and maps OpenAPI and Scalar API documentation.
/// </summary>
public static class OpenApiRegistrationExtensions
{
    /// <summary>
    /// Adds OpenAPI documentation to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi();
        return services;
    }

    /// <summary>
    /// Maps OpenAPI document and, in Development, Scalar API reference UI.
    /// Call after middleware, before Razor Components.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The route builder for chaining.</returns>
    public static IEndpointRouteBuilder MapOpenApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi()
            .AllowAnonymous();

        if (app.Environment.IsDevelopment())
        {
            app.MapScalarApiReference(opt =>
                opt
                    .EnableDarkMode()
                    .WithTheme(ScalarTheme.Mars)
                    .WithTitle("Blazor Auto VSA API Documentation"))
                .AllowAnonymous();
        }

        return app;
    }
}
