using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;

namespace Server.Extensions;

public static class MiddlewarePipelineExtensions
{
    private const string DefaultCultureName = "it-IT";

    public static WebApplication UseAppMiddleware(this WebApplication app)
    {
        app.UseDatabaseMigration();
        app.UseDatabaseSeeding();

        app.UseForwardedHeaders();

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

        if (app.Environment.IsDevelopment())
        {
            // Note: HTTPS redirection is disabled for Railway
            // Railway handles SSL termination at the edge

            app.UseHttpsRedirection();
        }

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

        return app;
    }
}
