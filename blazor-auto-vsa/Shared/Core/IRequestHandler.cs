namespace Shared.Core;

/// <summary>
/// Handler interface for processing requests.
/// Implement this interface for each request type to define its business logic.
/// </summary>
/// <typeparam name="TRequest">The type of request this handler processes.</typeparam>
/// <typeparam name="TResponse">The type of response this handler returns.</typeparam>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request and returns a response.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response.</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
}
