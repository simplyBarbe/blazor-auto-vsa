using Shared.Core;

namespace Client.Dispatching;

/// <summary>
/// Maps requests to their corresponding API endpoints.
/// </summary>
public class RequestEndpointMapper : IRequestEndpointMapper, IRouteMap
{
    private readonly Dictionary<Type, (string Template, HttpMethod Method)> _routes = new();

    public RequestEndpointMapper(IEnumerable<IRouteDefinition> definitions)
    {
        foreach (var definition in definitions)
        {
            definition.Define(this);
        }
    }

    public void Map<TRequest>(string template, HttpMethod method) where TRequest : class
    {
        _routes[typeof(TRequest)] = (template, method);
    }

    public (string Url, HttpMethod Method) GetEndpoint<TResponse>(IRequest<TResponse> request)
    {
        var type = request.GetType();
        if (!_routes.TryGetValue(type, out var mapping))
        {
            throw new InvalidOperationException($"No route mapped for {type.Name}");
        }

        var url = mapping.Template;
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Simple logic to replace placeholders like {Id} with property values from the request
        foreach (var prop in type.GetProperties())
        {
            var value = prop.GetValue(request)?.ToString();
            if (value == null) continue;

            var placeholder = $"{{{prop.Name}}}";
            if (url.Contains(placeholder, StringComparison.OrdinalIgnoreCase))
            {
                url = url.Replace(placeholder, Uri.EscapeDataString(value), StringComparison.OrdinalIgnoreCase);
                used.Add(prop.Name);
            }
        }

        if (mapping.Method == HttpMethod.Get)
        {
            var queryParts = type.GetProperties()
                .Select(p => (p.Name, Value: p.GetValue(request)?.ToString()))
                .Where(p => p.Value != null && !used.Contains(p.Name))
                .Select(p => $"{Uri.EscapeDataString(p.Name)}={Uri.EscapeDataString(p.Value!)}")
                .ToList();

            if (queryParts.Count > 0)
            {
                var separator = url.Contains('?') ? "&" : "?";
                url = $"{url}{separator}{string.Join("&", queryParts)}";
            }
        }

        return (url, mapping.Method);
    }
}
