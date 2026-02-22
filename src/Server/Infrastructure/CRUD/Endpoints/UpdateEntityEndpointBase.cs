using Microsoft.AspNetCore.Mvc;
using Server.Infrastructure.Endpoints;
using Shared.Core;
using Shared.Core.CRUD;
using Shared.Core.Exceptions;
using Shared.Core.Validation;

namespace Server.Infrastructure.CRUD.Endpoints;

/// <summary>Base for update endpoints. Supports "id" or composite keys via GetRoute.</summary>
public abstract class UpdateEntityEndpointBase<TKey, TCommand, TResponse> : IEndpoint
    where TCommand : IRequest<TResponse>
{
    public virtual void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(GetRoute(), HandleAsync)
            .WithTags(EndpointTagHelper.GetFeatureTag(GetType()));
    }

    protected abstract string GetRoute();

    protected virtual async Task<IResult> HandleAsync(
        TKey key,
        [FromBody] TCommand command,
        IRequestSender sender,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await sender.Send<TResponse>(command, cancellationToken);
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
