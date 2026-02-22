using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using Server.Components;

namespace Server.Extensions;

public static class WebUiRegistrationExtensions
{
    public static IServiceCollection AddWebUiServices(this IServiceCollection services)
    {
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
