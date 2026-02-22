using Client.Extensions;
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

            CultureConfiguration.SetDefault();

            builder.Services.AddFluentUIComponents();
            builder.AddApiHttpClient();

            builder.Services.AddInfrastructure(
                typeof(Program).Assembly,
                typeof(CreateProductCommandValidator).Assembly);

            builder.AddClientAuthentication();

            await builder.Build().RunAsync();
        }
    }
}
