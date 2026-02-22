using FluentValidation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Server.Infrastructure.Dispatching;
using Server.Infrastructure.Common;
using Server.Infrastructure.Services;
using Shared.Core;
using System.Reflection;

namespace Server.Extensions;

public static class InfrastructureExtensions
{
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddScoped<IDateTimeProvider, DateTimeProvider>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            services.AddRequestPipeline();
            services.AddScoped<IRequestSender, LocalRequestSender>();

            foreach (var assembly in assemblies)
        {
            services.AddHandlersFromAssembly(assembly);
            services.AddValidatorsFromAssembly(assembly);
        }

        return services;
    }

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            app.MapApiEndpoints(assembly);
        }

        return app;
    }
}
