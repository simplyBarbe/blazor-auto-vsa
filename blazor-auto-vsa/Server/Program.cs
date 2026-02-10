using FluentValidation;
using Microsoft.FluentUI.AspNetCore.Components;
using Server.Components;
using Server.Extensions;
using Server.Features.Products.Create;
using Server.Features.Products.Get;
using Server.Infrastructure.Dispatching;
using Server.Infrastructure.Mapping;
using Shared.Core;
using System.Globalization;

namespace Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var defaultCulture = new CultureInfo("it-IT");
            CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();
            builder.Services.AddFluentUIComponents();
            builder.Services.AddLocalization();

            // Register server infrastructure (Handlers, Validators, Pipeline, Dispatcher)
            builder.Services.AddInfrastructure(
                typeof(Program).Assembly, 
                typeof(Shared.Features.Products.Create.CreateProductCommandValidator).Assembly);

            // Register database context, unit of work, and AutoMapper
            builder.Services.AddApplicationDbContext(builder.Configuration, typeof(ProductMappingProfile).Assembly);

            var app = builder.Build();

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

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            // Map API endpoints for WebAssembly client
            app.MapEndpoints(typeof(Program).Assembly);

            app.Run();
        }
    }
}
