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
        
        // Simple logic to replace placeholders like {Id} with property values from the request
        foreach (var prop in type.GetProperties())
        {
            var placeholder = $"{{{prop.Name}}}";
            if (url.Contains(placeholder, StringComparison.OrdinalIgnoreCase))
            {
                var value = prop.GetValue(request)?.ToString();
                url = url.Replace(placeholder, Uri.EscapeDataString(value ?? string.Empty), StringComparison.OrdinalIgnoreCase);
            }
        }

        return (url, mapping.Method);
    }
}
