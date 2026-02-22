using Microsoft.AspNetCore.Mvc;
using Server.Infrastructure.Endpoints;
using Shared.Core;
using Shared.Core.CRUD;
using Shared.Core.Exceptions;
using Shared.Core.Validation;

namespace Server.Infrastructure.CRUD.Endpoints;

/// <summary>Base for get-by-key endpoints. Supports "id" or composite keys via GetRoute/CreateQuery.</summary>
public abstract class GetEntityEndpointBase<TKey, TQuery, TResponse> : IEndpoint
    where TQuery : IRequest<TResponse>
{
    public virtual void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(GetRoute(), HandleAsync)
           .WithTags(EndpointTagHelper.GetFeatureTag(GetType()))
           .DisableAntiforgery();
    }

    protected abstract string GetRoute();
    protected abstract TQuery CreateQuery(TKey key);

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
