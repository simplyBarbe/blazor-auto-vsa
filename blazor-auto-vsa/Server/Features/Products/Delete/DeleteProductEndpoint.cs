using Server.Infrastructure.CRUD.Endpoints;
using Shared.Features.Products.Delete;

namespace Server.Features.Products.Delete;

/// <summary>
/// Endpoint for deleting a product.
/// </summary>
public class DeleteProductEndpoint : DeleteEntityEndpointBase<int, DeleteProductCommand>
{
    /// <inheritdoc />
    protected override string GetRoute() => "/api/products/{id:int}";

    /// <inheritdoc />
    protected override DeleteProductCommand CreateCommand(int key)
    {
        return new DeleteProductCommand(key);
    }
}
