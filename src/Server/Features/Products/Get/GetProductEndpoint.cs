using Server.Infrastructure.CRUD.Endpoints;
using Shared.Features.Products.Get;
using Shared.Features.Products.Responses;

namespace Server.Features.Products.Get;

public class GetProductEndpoint : GetEntityEndpointBase<int, GetProductQuery, ProductResponse>
{
    protected override string GetRoute() => "/api/products/{key:int}";

    protected override GetProductQuery CreateQuery(int key)
    {
        return new GetProductQuery(key);
    }
}
