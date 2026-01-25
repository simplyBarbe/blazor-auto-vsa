using Shared.Core;

namespace Client.Dispatching;

/// <summary>
/// Interface for mapping requests to their HTTP endpoints.
/// </summary>
public interface IRequestEndpointMapper
{
    /// <summary>
    /// Gets the endpoint URL and HTTP method for a given request.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected.</typeparam>
    /// <param name="request">The request to map.</param>
    /// <returns>A tuple containing the URL and HTTP method.</returns>
    (string Url, HttpMethod Method) GetEndpoint<TResponse>(IRequest<TResponse> request);
}
