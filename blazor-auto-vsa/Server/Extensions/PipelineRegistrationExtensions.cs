using Server.Infrastructure.Pipeline;
using Shared.Core.Pipeline;

namespace Server.Extensions;

/// <summary>
/// Extension methods for registering pipeline behaviors.
/// </summary>
public static class PipelineRegistrationExtensions
{
    /// <summary>
    /// Registers the request pipeline with validation behavior.
    /// </summary>
    public static IServiceCollection AddRequestPipeline(this IServiceCollection services)
    {
        // Register the validator adapter
        services.AddScoped<Shared.Core.Validation.IAsyncRequestValidator, Shared.Core.Validation.FluentValidationRequestValidator>();

        // Register pipeline behaviors (order matters - first registered = first executed)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
