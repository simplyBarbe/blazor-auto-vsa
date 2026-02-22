using Server.Infrastructure.CRUD.Endpoints;
using Shared.Features.Products.Create;
using Shared.Features.Products.Responses;

namespace Server.Features.Products.Create;

public class CreateProductEndpoint : CreateEntityEndpointBase<CreateProductCommand, ProductResponse>
{
    protected override string GetRoute() => "/api/products";

    protected override string GetCreatedLocation(object result)
    {
        if (result is ProductResponse productResponse)
        {
            return $"/api/products/{productResponse.Id}";
        }
        return base.GetCreatedLocation(result);
    }
}
