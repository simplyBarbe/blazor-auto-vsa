using Server.Infrastructure.Endpoints;
using Shared.Core;
using Shared.Core.Validation;
using Shared.Features.Products.Create;

namespace Server.Features.Products.Create;

/// <summary>
/// Endpoint for creating a new product.
/// </summary>
public class CreateProductEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the POST product endpoint.
    /// </summary>
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/products", HandleAsync)
            .DisableAntiforgery();
    }

    private async Task<IResult> HandleAsync(
        CreateProductCommand command,
        IRequestSender sender,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/products/{result.Id}", result);
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new ValidationErrorResponse { Errors = ex.Errors });
        }
    }
}
