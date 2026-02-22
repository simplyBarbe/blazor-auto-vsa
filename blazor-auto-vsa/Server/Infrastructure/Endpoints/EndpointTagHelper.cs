namespace Server.Infrastructure.Endpoints;

public static class EndpointTagHelper
{
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
