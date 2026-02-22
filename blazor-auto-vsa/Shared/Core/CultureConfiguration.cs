using System.Globalization;

namespace Shared.Core;

public static class CultureConfiguration
{
    public static void SetDefault(string cultureName = "it-IT")
    {
        var culture = new CultureInfo(cultureName);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
