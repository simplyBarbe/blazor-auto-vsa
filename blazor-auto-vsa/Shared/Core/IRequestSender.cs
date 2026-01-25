namespace Shared.Core;

/// <summary>
/// Dispatcher interface for sending requests.
/// Components use this to send requests without knowing if they run locally or via HTTP.
/// </summary>
public interface IRequestSender
{
    /// <summary>
    /// Sends a request and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
