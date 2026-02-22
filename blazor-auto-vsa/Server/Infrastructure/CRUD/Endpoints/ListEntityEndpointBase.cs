using Microsoft.AspNetCore.Mvc;
using Server.Infrastructure.Endpoints;
using Shared.Core;
using Shared.Core.CRUD;
using Shared.Core.Validation;

namespace Server.Infrastructure.CRUD.Endpoints;

public abstract class ListEntityEndpointBase<TQuery, TResponse> : IEndpoint
    where TQuery : IRequest<PagedResult<TResponse>>
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(GetRoute(), HandleAsync)
            .WithTags(EndpointTagHelper.GetFeatureTag(GetType()))
            .DisableAntiforgery();
    }

    protected abstract string GetRoute();

    protected virtual TQuery CreateQuery(TQuery query) => query;

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
