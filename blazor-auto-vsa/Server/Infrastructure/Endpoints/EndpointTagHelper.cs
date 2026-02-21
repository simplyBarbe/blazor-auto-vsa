namespace Server.Infrastructure.Endpoints;

/// <summary>
/// Resolves OpenAPI tags for endpoints based on the endpoint type namespace.
/// </summary>
public static class EndpointTagHelper
{
    /// <summary>
    /// Gets the feature tag from endpoint type namespace, falling back to endpoint type name.
    /// </summary>
    /// <param name="endpointType">The endpoint type.</param>
    /// <returns>The OpenAPI tag to use.</returns>
    public static string GetFeatureTag(Type endpointType)
    {
        var namespaceParts = endpointType.Namespace?.Split('.') ?? [];
        var featureIndex = Array.IndexOf(namespaceParts, "Features");

        if (featureIndex >= 0 && featureIndex + 1 < namespaceParts.Length)
        {
            return namespaceParts[featureIndex + 1];
        }

        var name = endpointType.Name;
        var genericMarkerIndex = name.IndexOf('`');
        if (genericMarkerIndex >= 0)
        {
            name = name[..genericMarkerIndex];
        }

        if (name.EndsWith("Endpoint", StringComparison.Ordinal))
        {
            name = name[..^"Endpoint".Length];
        }

        return name;
    }
}
