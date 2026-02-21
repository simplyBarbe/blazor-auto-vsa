using System.Globalization;
using FluentValidation;
using Microsoft.FluentUI.AspNetCore.Components;
using Scalar.AspNetCore;
using Server.Components;
using Server.Extensions;
using Server.Features.Products.Create;
using Server.Features.Products.Get;
using Server.Infrastructure.Dispatching;
using Server.Infrastructure.Mapping;
using Shared.Core;
using Serilog;

namespace Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting web application");

                var builder = WebApplication.CreateBuilder(args);
                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext());

                builder.Services.AddSerilog();

                var defaultCulture = new CultureInfo("it-IT");
                CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
                CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

                // Add services to the container.
                var razorComponents = builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();
                razorComponents.AddAuthenticationStateSerialization();

                builder.Services.AddFluentUIComponents();
                builder.Services.AddLocalization();

                builder.Services.AddHttpContextAccessor();
                builder.Services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
                builder.Services.AddHttpClient();
                builder.Services.AddScoped(sp => new HttpClient());

                // Register server infrastructure (Handlers, Validators, Pipeline, Dispatcher)
                builder.Services.AddInfrastructure(
                    typeof(Program).Assembly,
                    typeof(Shared.Features.Products.Create.CreateProductCommandValidator).Assembly);

                // Register database context, unit of work, and AutoMapper
                builder.Services.AddApplicationDbContext(builder.Configuration, typeof(ProductMappingProfile).Assembly);

                // Register authentication and authorization services
                builder.Services.AddAuthenticationAndAuthorization();

                // Add cascading authentication state for server-side rendering
                builder.Services.AddCascadingAuthenticationState();

                // Add OpenAPI documentation
                builder.Services.AddOpenApi();

                var app = builder.Build();

                app.UseSerilogRequestLogging();

                // Apply pending database migrations
                app.UseDatabaseMigration();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseWebAssemblyDebugging();
                }
                else
                {
                    app.UseExceptionHandler("/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
                app.UseHttpsRedirection();

                app.UseAuthentication();
                app.UseAuthorization();

                app.UseRequestLocalization(new RequestLocalizationOptions
                {
                    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(defaultCulture),
                    SupportedCultures = new[] { defaultCulture },
                    SupportedUICultures = new[] { defaultCulture }
                });

                app.UseAntiforgery();

                if (!app.Environment.IsEnvironment("Testing"))
                {
                    app.MapStaticAssets();
                }

                // Map OpenAPI documentation (must be after middleware, before Razor Components)
                //app.MapOpenApi("/openapi/{documentName}.json")
                app.MapOpenApi()
                    .AllowAnonymous();

                if (app.Environment.IsDevelopment())
                {

                    app.MapScalarApiReference(opt =>
                        opt
                        .EnableDarkMode()
                        .WithTheme(ScalarTheme.Mars)
                            .WithTitle("Blazor Auto VSA API Documentation")
                    )
                    .AllowAnonymous();
                }

                // Map API endpoints for WebAssembly client (must be before Razor Components)
                app.MapEndpoints(typeof(Program).Assembly);

                app.MapRazorComponents<App>()
                    .AddInteractiveServerRenderMode()
                    .AddInteractiveWebAssemblyRenderMode()
                    .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

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
