using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Client.Extensions;

public static class ApiHttpClientExtensions
{
    public static WebAssemblyHostBuilder AddApiHttpClient(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddScoped<Client.Infrastructure.Auth.AntiforgeryHandler>();

        builder.Services.AddHttpClient("API", client =>
        {
            client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
        })
            .AddHttpMessageHandler<Client.Infrastructure.Auth.AntiforgeryHandler>();

        builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

        return builder;
    }
}
