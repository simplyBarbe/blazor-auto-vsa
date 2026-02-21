using Server.Extensions;
using Server.Features.Products.Get;
using Server.Infrastructure.Mapping;
using Serilog;
using Serilog.AspNetCore;
using Shared.Core;
using Shared.Features.Products.Create;

namespace Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SerilogExtensions.UseSerilogBootstrap();

            try
            {
                Log.Information("Starting web application");

                var builder = WebApplication.CreateBuilder(args);
                builder.ConfigureSerilog();

                CultureConfiguration.SetDefault();

                builder.Services.AddWebUiServices();

                builder.Services.AddInfrastructure(
                    typeof(Program).Assembly,
                    typeof(CreateProductCommandValidator).Assembly);

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
