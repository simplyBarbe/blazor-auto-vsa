using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using Server.Components;

namespace Server.Extensions;

public static class WebUiRegistrationExtensions
{
    public static IServiceCollection AddWebUiServices(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.
            KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        var razorComponents = services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();
        razorComponents.AddAuthenticationStateSerialization();

        services.AddFluentUIComponents();
        services.AddLocalization();

        services.AddHttpContextAccessor();
        services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
        services.AddHttpClient();
        services.AddScoped(_ => new HttpClient());

        return services;
    }
}
