namespace Client.Dispatching;

/// <summary>
/// A feature registers its client-side HTTP routes by implementing this interface.
/// Implementations are discovered by assembly scan in AddInfrastructure.
/// </summary>
public interface IRouteDefinition
{
    void Define(RequestEndpointMapper routes);
}
