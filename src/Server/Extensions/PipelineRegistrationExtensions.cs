using Server.Infrastructure.Pipeline;
using Shared.Core.Pipeline;

namespace Server.Extensions;

public static class PipelineRegistrationExtensions
{
    public static IServiceCollection AddRequestPipeline(this IServiceCollection services)
    {
        services.AddScoped<Shared.Core.Validation.IAsyncRequestValidator, Shared.Core.Validation.FluentValidationRequestValidator>();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
