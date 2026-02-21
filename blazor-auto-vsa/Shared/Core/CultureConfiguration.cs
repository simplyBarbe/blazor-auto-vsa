using System.Globalization;

namespace Shared.Core;

/// <summary>
/// Central configuration for application default culture (DRY for Server and Client).
/// </summary>
public static class CultureConfiguration
{
    /// <summary>
    /// Sets the default thread culture and UI culture for the current process.
    /// </summary>
    /// <param name="cultureName">Culture name (e.g. "it-IT"). Defaults to "it-IT".</param>
    public static void SetDefault(string cultureName = "it-IT")
    {
        var culture = new CultureInfo(cultureName);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
