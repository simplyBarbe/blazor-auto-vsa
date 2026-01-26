using Microsoft.AspNetCore.Mvc;
using Server.Infrastructure.Endpoints;
using Shared.Core;
using Shared.Core.CRUD;
using Shared.Core.Exceptions;
using Shared.Core.Validation;

namespace Server.Infrastructure.CRUD.Endpoints;

/// <summary>
/// Abstract base endpoint for getting an entity by key.
/// Defaults to using "id" parameter (int or Guid), but can be easily overridden for custom parameter names or composite keys.
/// </summary>
/// <typeparam name="TKey">The key type for route binding (e.g., int, Guid, string).</typeparam>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public abstract class GetEntityEndpointBase<TKey, TQuery, TResponse> : IEndpoint
    where TQuery : IRequest<TResponse>
{
    /// <inheritdoc />
    public virtual void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(GetRoute(), HandleAsync)
           .DisableAntiforgery();
    }

    /// <summary>
    /// Gets the route pattern with key parameter. Must be implemented by derived classes.
    /// </summary>
    /// <returns>The route pattern (e.g., "/api/products/{id:int}" or "/api/orders/{year:int}/{month}" for composite keys).</returns>
    protected abstract string GetRoute();

    /// <summary>
    /// Creates a query from the key. Can be overridden for custom query creation.
    /// </summary>
    /// <param name="key">The key (will be converted to object for composite key support).</param>
    /// <returns>The query.</returns>
    protected abstract TQuery CreateQuery(TKey key);

    /// <summary>
    /// Handles the get request. Can be overridden for custom handling.
    /// </summary>
    /// <param name="key">The key from route binding.</param>
    /// <param name="sender">The request sender.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    protected virtual async Task<IResult> HandleAsync(
        TKey key,
        IRequestSender sender,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = CreateQuery(key);
            var result = await sender.Send<TResponse>(query, cancellationToken);
            return Results.Ok(result);
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new ValidationErrorResponse { Errors = ex.Errors });
        }
        catch (EntityNotFoundException)
        {
            return Results.NotFound();
        }
    }
}
