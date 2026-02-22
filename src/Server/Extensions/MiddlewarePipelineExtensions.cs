using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;

namespace Server.Extensions;

public static class MiddlewarePipelineExtensions
{
    private const string DefaultCultureName = "it-IT";

    public static WebApplication UseAppMiddleware(this WebApplication app)
    {
        app.UseDatabaseMigration();

        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        var defaultCulture = new CultureInfo(DefaultCultureName);
        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(defaultCulture),
            SupportedCultures = [defaultCulture],
            SupportedUICultures = [defaultCulture]
        });

        app.UseAntiforgery();

        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.MapStaticAssets();
        }

        return app;
    }
}
