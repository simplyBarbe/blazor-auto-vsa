using Microsoft.AspNetCore.Mvc;
using Server.Infrastructure.Endpoints;
using Shared.Core;
using Shared.Core.CRUD;
using Shared.Core.Exceptions;
using Shared.Core.Validation;

namespace Server.Infrastructure.CRUD.Endpoints;

/// <summary>
/// Abstract base endpoint for deleting entities.
/// Defaults to using "id" parameter (int or Guid), but can be easily overridden for custom parameter names or composite keys.
/// </summary>
/// <typeparam name="TKey">The key type for route binding (e.g., int, Guid, string).</typeparam>
/// <typeparam name="TCommand">The command type.</typeparam>
public abstract class DeleteEntityEndpointBase<TKey, TCommand> : IEndpoint
    where TCommand : IRequest<object?>
{
    /// <inheritdoc />
    public virtual void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete(GetRoute(), HandleAsync);
    }

    /// <summary>
    /// Gets the route pattern with key parameter. Must be implemented by derived classes.
    /// </summary>
    /// <returns>The route pattern (e.g., "/api/products/{id:int}" or "/api/orders/{year:int}/{month}" for composite keys).</returns>
    protected abstract string GetRoute();

    /// <summary>
    /// Creates a command from the key. Can be overridden for custom command creation.
    /// </summary>
    /// <param name="key">The key (will be converted to object for composite key support).</param>
    /// <returns>The command.</returns>
    protected abstract TCommand CreateCommand(TKey key);

    /// <summary>
    /// Handles the delete request. Can be overridden for custom handling.
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
            var command = CreateCommand(key);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
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
