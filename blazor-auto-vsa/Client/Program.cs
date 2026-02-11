using System.Globalization;
using Client.Dispatching;
using Client.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Shared.Core;
using Shared.Features.Products.Create;

namespace Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            var defaultCulture = new CultureInfo("it-IT");
            CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

            builder.Services.AddFluentUIComponents();

            // Register AntiforgeryHandler
            builder.Services.AddScoped<Client.Infrastructure.Auth.AntiforgeryHandler>();

            // Configure HttpClient for API calls with Antiforgery support
            builder.Services.AddHttpClient("API", client =>
            {
                client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            })
            .AddHttpMessageHandler<Client.Infrastructure.Auth.AntiforgeryHandler>();

            // Explicitly register HttpClient for standard injection
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

            // Register client infrastructure (Dispatcher, Validators, Route Definitions)
            builder.Services.AddInfrastructure(
                typeof(Program).Assembly,
                typeof(CreateProductCommandValidator).Assembly);

            // Register authentication services
            builder.AddClientAuthentication();

            await builder.Build().RunAsync();
        }
    }
}
