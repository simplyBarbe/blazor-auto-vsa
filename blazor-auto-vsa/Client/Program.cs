using Client.Dispatching;
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

            builder.Services.AddFluentUIComponents();

            // Configure HttpClient for API calls
            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            });

            // Register Smart Dispatcher for WebAssembly
            builder.Services.AddSingleton<IRequestEndpointMapper, RequestEndpointMapper>();
            builder.Services.AddScoped<IRequestSender, HttpRequestSender>();

            // Register FluentValidation validators from Shared (basic synchronous rules)
            builder.Services.AddValidatorsFromAssembly(typeof(CreateProductCommandValidator).Assembly);

            await builder.Build().RunAsync();
        }
    }
}
