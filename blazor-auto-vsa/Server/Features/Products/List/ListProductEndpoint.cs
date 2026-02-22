using Server.Infrastructure.CRUD.Endpoints;
using Shared.Features.Products.List;
using Shared.Features.Products.Responses;

namespace Server.Features.Products.List;

public class ListProductEndpoint : ListEntityEndpointBase<ListProductQuery, ProductResponse>
{
    protected override string GetRoute() => "/api/products";
}
