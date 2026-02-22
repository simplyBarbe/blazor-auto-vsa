namespace Client.Dispatching;

public interface IRouteDefinition
{
    void Define(IRouteMap map);
}

public interface IRouteMap
{
    void Map<TRequest>(string template, HttpMethod method) where TRequest : class;
}
