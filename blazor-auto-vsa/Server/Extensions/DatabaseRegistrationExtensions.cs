using Microsoft.EntityFrameworkCore;
using Server.Infrastructure.Data;
using Server.Infrastructure.Data.Contracts;
using Server.Infrastructure.Data.Repositories;
using System.Reflection;
using Server.Infrastructure.Data.Interceptors;

namespace Server.Extensions;

/// <summary>
/// Extension methods for registering database services in DI.
/// </summary>
public static class DatabaseRegistrationExtensions
{
    /// <summary>
    /// Registers the application database context, unit of work, and AutoMapper.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="mappingAssemblies">Assemblies to scan for AutoMapper profiles.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration configuration, params Assembly[] mappingAssemblies)
    {
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<AuditableEntityInterceptor>()
            );
        });

        // Register AutoMapper with specified assemblies
        services.AddAutoMapper(mappingAssemblies);

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    /// <summary>
    /// Applies pending database migrations at startup.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseDatabaseMigration(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        return app;
    }
}
