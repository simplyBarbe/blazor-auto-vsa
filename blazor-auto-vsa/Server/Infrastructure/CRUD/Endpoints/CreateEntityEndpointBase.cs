using Microsoft.AspNetCore.Mvc;
using Server.Infrastructure.Endpoints;
using Shared.Core;
using Shared.Core.CRUD;
using Shared.Core.Validation;

namespace Server.Infrastructure.CRUD.Endpoints;

public abstract class CreateEntityEndpointBase<TCommand, TResponse> : IEndpoint
    where TCommand : IRequest<TResponse>
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(GetRoute(), HandleAsync)
            .WithTags(EndpointTagHelper.GetFeatureTag(GetType()));
    }

    protected abstract string GetRoute();

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

    protected virtual string GetCreatedLocation(object result)
    {
        return GetRoute();
    }
}
