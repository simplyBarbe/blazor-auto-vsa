using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;

namespace Server.Extensions;

/// <summary>
/// Configures the HTTP request pipeline (middleware order) in one place.
/// </summary>
public static class MiddlewarePipelineExtensions
{
    private const string DefaultCultureName = "it-IT";

    /// <summary>
    /// Configures database migration, environment-specific middleware,
    /// status pages, HTTPS, auth, localization, antiforgery, and conditional static assets.
    /// Call app.UseSerilogRequestLogging() before this if using Serilog.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
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
