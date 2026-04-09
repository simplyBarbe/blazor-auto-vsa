using Server.Extensions;
using Server.Infrastructure.Mapping;
using Serilog;
using Serilog.AspNetCore;
using Shared.Core;
using Shared.Features.Products.Create;

namespace Server
{
    /// <summary>
    /// Entry point for the application. Made partial so WebApplicationFactory can discover and bootstrap the app in integration tests.
    /// </summary>
    public partial class Program
    {
        public static void Main(string[] args)
        {
            SerilogExtensions.UseSerilogBootstrap();

            try
            {
                Log.Information("Starting web application");

                var builder = WebApplication.CreateBuilder(args);
                builder.ConfigureSerilog();
                
                // Configure Railway Port
                var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
                builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

                CultureConfiguration.SetDefault();

                builder.Services.AddWebUiServices();

                // Register Shared validators first, then Server, so server validators that wrap Shared rules
                // (same IValidator<T>) win in DI over the standalone Shared validators.
                builder.Services.AddInfrastructure(
                    typeof(CreateProductCommandValidator).Assembly,
                    typeof(Program).Assembly);

                builder.Services.AddApplicationDbContext(builder.Configuration, typeof(ProductMappingProfile).Assembly);

                builder.Services.AddAuthenticationAndAuthorization();
                builder.Services.AddCascadingAuthenticationState();

                builder.Services.AddOpenApiDocumentation();

                var app = builder.Build();

                app.UseSerilogRequestLogging();
                app.UseAppMiddleware();
                app.MapApp(typeof(Program).Assembly);

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
