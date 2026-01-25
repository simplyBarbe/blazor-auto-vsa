using Microsoft.AspNetCore.Mvc;
using Server.Infrastructure.Endpoints;
using Shared.Core;
using Shared.Core.CRUD;
using Shared.Core.Validation;

namespace Server.Infrastructure.CRUD.Endpoints;

/// <summary>
/// Abstract base endpoint for listing entities.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public abstract class ListEntityEndpointBase<TQuery, TResponse> : IEndpoint
    where TQuery : IRequest<PagedResult<TResponse>>
{
    /// <inheritdoc />
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(GetRoute(), HandleAsync)
            .DisableAntiforgery();
    }

    /// <summary>
    /// Gets the route pattern. Must be implemented by derived classes.
    /// </summary>
    /// <returns>The route pattern (e.g., "/api/products").</returns>
    protected abstract string GetRoute();

    /// <summary>
    /// Creates a query from the request. Can be overridden for query string parameter binding.
    /// </summary>
    /// <param name="query">The query from model binding.</param>
    /// <returns>The query to use.</returns>
    protected virtual TQuery CreateQuery(TQuery query)
    {
        return query;
    }

    /// <summary>
    /// Handles the list request. Can be overridden for custom handling.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="sender">The request sender.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    protected virtual async Task<IResult> HandleAsync(
        [AsParameters] TQuery query,
        [FromServices] IRequestSender sender,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var finalQuery = CreateQuery(query);
            var result = await sender.Send<PagedResult<TResponse>>(finalQuery, cancellationToken);
            return Results.Ok(result);
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new ValidationErrorResponse { Errors = ex.Errors });
        }
    }
}
