using Server.Infrastructure.CRUD.Endpoints;
using Shared.Features.Products.List;
using Shared.Features.Products.Responses;

namespace Server.Features.Products.List;

/// <summary>
/// Endpoint for listing products.
/// </summary>
public class ListProductEndpoint : ListEntityEndpointBase<ListProductQuery, ProductResponse>
{
    /// <inheritdoc />
    protected override string GetRoute() => "/api/products";
}
