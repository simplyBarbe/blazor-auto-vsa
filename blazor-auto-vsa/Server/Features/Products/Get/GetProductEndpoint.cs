using Server.Infrastructure.CRUD.Endpoints;
using Shared.Features.Products.Get;
using Shared.Features.Products.Responses;

namespace Server.Features.Products.Get;

/// <summary>
/// Endpoint for retrieving a product by ID.
/// </summary>
public class GetProductEndpoint : GetEntityEndpointBase<int, GetProductQuery, ProductResponse>
{
    /// <inheritdoc />
    protected override string GetRoute() => "/api/products/{id:int}";

    /// <inheritdoc />
    protected override GetProductQuery CreateQuery(int key)
    {
        return new GetProductQuery(key);
    }
}
