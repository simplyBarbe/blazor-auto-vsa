using Server.Infrastructure.Endpoints;
using Shared.Core;
using Shared.Core.Validation;
using Shared.Features.Products.Get;

namespace Server.Features.Products.Get;

/// <summary>
/// Endpoint for retrieving a product by ID.
/// </summary>
public class GetProductEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the GET product endpoint.
    /// </summary>
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products/{id:int}", HandleAsync)
            .DisableAntiforgery();
    }

    private async Task<IResult> HandleAsync(
        int id,
        IRequestSender sender,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await sender.Send(new GetProductQuery(id), cancellationToken);
            return Results.Ok(result);
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new ValidationErrorResponse { Errors = ex.Errors });
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
    }
}
