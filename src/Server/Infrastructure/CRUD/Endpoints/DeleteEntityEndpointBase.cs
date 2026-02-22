using Microsoft.AspNetCore.Mvc;
using Server.Infrastructure.Endpoints;
using Shared.Core;
using Shared.Core.CRUD;
using Shared.Core.Exceptions;
using Shared.Core.Validation;

namespace Server.Infrastructure.CRUD.Endpoints;

/// <summary>Base for delete endpoints. Supports "id" or composite keys via GetRoute/CreateCommand.</summary>
public abstract class DeleteEntityEndpointBase<TKey, TCommand> : IEndpoint
    where TCommand : IRequest<object?>
{
    public virtual void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete(GetRoute(), HandleAsync)
            .WithTags(EndpointTagHelper.GetFeatureTag(GetType()));
    }

    protected abstract string GetRoute();
    protected abstract TCommand CreateCommand(TKey key);

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
