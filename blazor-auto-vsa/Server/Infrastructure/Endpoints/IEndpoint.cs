namespace Server.Infrastructure.Endpoints;

/// <summary>
/// Interface for API endpoints that can be automatically discovered and registered.
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the route builder.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    void Map(IEndpointRouteBuilder app);
}
