namespace Shared.Core.Pipeline;

/// <summary>
/// Delegate representing the next step in the pipeline.
/// </summary>
/// <typeparam name="TResponse">The response type.</typeparam>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Pipeline behavior that can intercept and process requests before/after handler execution.
/// Behaviors are executed in the order they are registered.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : Shared.Core.IRequest<TResponse>
{
    /// <summary>
    /// Handles the request and optionally passes it to the next behavior in the pipeline.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="next">Delegate to invoke the next behavior in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response from the pipeline.</returns>
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}
