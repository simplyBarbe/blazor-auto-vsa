using Shared.Core;
using Shared.Core.Pipeline;

namespace Server.Infrastructure.Dispatching;

/// <summary>
/// Implementation of IRequestSender that invokes handlers through a pipeline.
/// Used during SSR/Prerendering when running on the server.
/// </summary>
public class LocalRequestSender : IRequestSender
{
    private readonly IServiceScopeFactory _scopeFactory;

    public LocalRequestSender(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        
        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        // Get all pipeline behaviors for this request/response type
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
        var behaviors = serviceProvider.GetServices(behaviorType).Cast<object>().ToList();

        // Build the final handler delegate
        RequestHandlerDelegate<TResponse> handler = () => ExecuteHandler<TResponse>(serviceProvider, request, cancellationToken);

        // Build the pipeline by wrapping behaviors from last to first
        foreach (var behavior in behaviors.AsEnumerable().Reverse())
        {
            var currentBehavior = behavior;
            var next = handler;

            handler = () =>
            {
                var handleMethod = currentBehavior.GetType().GetMethod("Handle")
                    ?? throw new InvalidOperationException($"Handle method not found on {currentBehavior.GetType().Name}");

                return (Task<TResponse>)handleMethod.Invoke(currentBehavior, [request, next, cancellationToken])!;
            };
        }

        return await handler();
    }

    private async Task<TResponse> ExecuteHandler<TResponse>(IServiceProvider serviceProvider, IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = serviceProvider.GetRequiredService(handlerType);

        var method = handlerType.GetMethod("Handle")
            ?? throw new InvalidOperationException($"Handle method not found on {handlerType.Name}");

        var result = method.Invoke(handler, [request, cancellationToken]);

        return await (Task<TResponse>)result!;
    }
}
