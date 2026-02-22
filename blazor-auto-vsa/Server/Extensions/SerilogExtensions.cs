using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Server.Extensions;

public static class SerilogExtensions
{
    /// Call once at application startup before building the host.
    public static void UseSerilogBootstrap()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
            .CreateBootstrapLogger();
    }

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
