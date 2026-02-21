using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Server.Extensions;

/// <summary>
/// Centralizes Serilog bootstrap and host configuration for the server.
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Creates and assigns the bootstrap logger (Console with CompactJsonFormatter).
    /// Call once at application startup before building the host.
    /// </summary>
    public static void UseSerilogBootstrap()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
            .CreateBootstrapLogger();
    }

    /// <summary>
    /// Configures Serilog on the host and adds it to the service collection.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

        builder.Services.AddSerilog();

        return builder;
    }
}
