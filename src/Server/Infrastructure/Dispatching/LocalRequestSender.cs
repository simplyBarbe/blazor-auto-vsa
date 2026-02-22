using System.Collections.Concurrent;
using System.Linq.Expressions;
using Shared.Core;
using Shared.Core.Pipeline;

namespace Server.Infrastructure.Dispatching;

/// <summary>Invokes handlers through the pipeline. Used for SSR/Prerendering on the server.</summary>
public class LocalRequestSender : IRequestSender
{
    private readonly IServiceScopeFactory _scopeFactory;

    private static readonly ConcurrentDictionary<Type, Func<object, object, CancellationToken, Task<object>>> _handlerDelegateCache = new();
    private static readonly ConcurrentDictionary<Type, Func<object, object, object, CancellationToken, Task<object>>> _behaviorDelegateCache = new();

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

        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
        var behaviors = serviceProvider.GetServices(behaviorType).Cast<object>().ToList();

        RequestHandlerDelegate<TResponse> handler = async () => 
        {
            var result = await ExecuteHandlerInternal(serviceProvider, request, requestType, responseType, cancellationToken);
            return (TResponse)result;
        };

        foreach (var behavior in behaviors.AsEnumerable().Reverse())
        {
            var currentBehavior = behavior;
            var next = handler;

            var behaviorDelegate = GetBehaviorDelegate(currentBehavior.GetType(), requestType, responseType);
            handler = async () => 
            {
                var result = await behaviorDelegate(currentBehavior, request, next, cancellationToken);
                return (TResponse)result;
            };
        }

        return await handler();
    }

    private async Task<object> ExecuteHandlerInternal(IServiceProvider serviceProvider, object request, Type requestType, Type responseType, CancellationToken cancellationToken)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        var handler = serviceProvider.GetRequiredService(handlerType);

        var handlerDelegate = GetHandlerDelegate(handlerType, requestType, responseType);
        return await handlerDelegate(handler, request, cancellationToken);
    }

    private static Func<object, object, CancellationToken, Task<object>> GetHandlerDelegate(Type handlerType, Type requestType, Type responseType)
    {
        return _handlerDelegateCache.GetOrAdd(handlerType, type =>
        {
            var handlerParam = Expression.Parameter(typeof(object), "handler");
            var requestParam = Expression.Parameter(typeof(object), "request");
            var ctParam = Expression.Parameter(typeof(CancellationToken), "ct");

            var method = type.GetMethod("Handle") 
                ?? throw new InvalidOperationException($"Handle method not found on {type.Name}");

            var call = Expression.Call(
                Expression.Convert(handlerParam, type),
                method,
                Expression.Convert(requestParam, requestType),
                ctParam
            );

            var convertedCall = Expression.Call(
                typeof(LocalRequestSender).GetMethod(nameof(ConvertTask), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(responseType),
                call
            );

            return Expression.Lambda<Func<object, object, CancellationToken, Task<object>>>(
                convertedCall, handlerParam, requestParam, ctParam).Compile();
        });
    }

    private static Func<object, object, object, CancellationToken, Task<object>> GetBehaviorDelegate(Type behaviorType, Type requestType, Type responseType)
    {
        return _behaviorDelegateCache.GetOrAdd(behaviorType, type =>
        {
            var behaviorParam = Expression.Parameter(typeof(object), "behavior");
            var requestParam = Expression.Parameter(typeof(object), "request");
            var nextParam = Expression.Parameter(typeof(object), "next");
            var ctParam = Expression.Parameter(typeof(CancellationToken), "ct");

            var method = type.GetMethod("Handle") 
                ?? throw new InvalidOperationException($"Handle method not found on {type.Name}");

            var nextDelegateType = typeof(RequestHandlerDelegate<>).MakeGenericType(responseType);

            var call = Expression.Call(
                Expression.Convert(behaviorParam, type),
                method,
                Expression.Convert(requestParam, requestType),
                Expression.Convert(nextParam, nextDelegateType),
                ctParam
            );

            var convertedCall = Expression.Call(
                typeof(LocalRequestSender).GetMethod(nameof(ConvertTask), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(responseType),
                call
            );

            return Expression.Lambda<Func<object, object, object, CancellationToken, Task<object>>>(
                convertedCall, behaviorParam, requestParam, nextParam, ctParam).Compile();
        });
    }

    private static async Task<object> ConvertTask<T>(Task<T> task)
    {
        var result = await task;
        return result!;
    }
}
