using Client.Dispatching;
using Client.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Shared.Core;
using Shared.Features.Products.Create;
using System.Globalization;

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

            // Configure HttpClient for API calls
            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            });

            // Register Smart Framework (Dispatcher, Validators, Route Definitions)
            builder.Services.AddSmartFramework(
                typeof(Program).Assembly, 
                typeof(CreateProductCommandValidator).Assembly);

            await builder.Build().RunAsync();
        }
    }
}
