using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace Server.Extensions;

public static class OpenApiRegistrationExtensions
{
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi();
        return services;
    }

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
