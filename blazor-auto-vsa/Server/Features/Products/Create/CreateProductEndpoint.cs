using Server.Infrastructure.CRUD.Endpoints;
using Shared.Features.Products.Create;
using Shared.Features.Products.Responses;

namespace Server.Features.Products.Create;

/// <summary>
/// Endpoint for creating a new product.
/// </summary>
public class CreateProductEndpoint : CreateEntityEndpointBase<CreateProductCommand, ProductResponse>
{
    /// <inheritdoc />
    protected override string GetRoute() => "/api/products";

    /// <inheritdoc />
    protected override string GetCreatedLocation(object result)
    {
        if (result is ProductResponse productResponse)
        {
            return $"/api/products/{productResponse.Id}";
        }
        return base.GetCreatedLocation(result);
    }
}
