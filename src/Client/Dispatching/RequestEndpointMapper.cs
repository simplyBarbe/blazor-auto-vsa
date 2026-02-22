using Shared.Core;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Client.Dispatching;

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

        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var url = Regex.Replace(mapping.Template, @"\{(?<name>\w+)(?::[^}]+)?\}", m =>
        {
            var name = m.Groups["name"].Value;
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            
            if (prop != null)
            {
                var val = prop.GetValue(request)?.ToString();
                if (val != null)
                {
                    used.Add(prop.Name);
                    return Uri.EscapeDataString(val);
                }
            }
            return m.Value; // Keep placeholder if no property match or value is null
        }, RegexOptions.IgnoreCase);

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
