using Microsoft.AspNetCore.Mvc;
using Server.Infrastructure.Endpoints;
using Shared.Core;
using Shared.Core.CRUD;
using Shared.Core.Validation;

namespace Server.Infrastructure.CRUD.Endpoints;

/// <summary>
/// Abstract base endpoint for creating entities.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public abstract class CreateEntityEndpointBase<TCommand, TResponse> : IEndpoint
    where TCommand : IRequest<TResponse>
{
    /// <inheritdoc />
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(GetRoute(), HandleAsync);
    }

    /// <summary>
    /// Gets the route pattern for the endpoint. Must be implemented by derived classes.
    /// </summary>
    /// <returns>The route pattern.</returns>
    protected abstract string GetRoute();

    /// <summary>
    /// Handles the create request. Can be overridden for custom handling.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="sender">The request sender.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    protected virtual async Task<IResult> HandleAsync(
        [FromBody] TCommand command,
        [FromServices] IRequestSender sender,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await sender.Send<TResponse>(command, cancellationToken);
            return Results.Created(GetCreatedLocation(result), result);
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new ValidationErrorResponse { Errors = ex.Errors });
        }
    }

    /// <summary>
    /// Gets the location header for the created entity. Can be overridden for custom location.
    /// </summary>
    /// <param name="result">The created entity result.</param>
    /// <returns>The location URL.</returns>
    protected virtual string GetCreatedLocation(object result)
    {
        return GetRoute();
    }
}
