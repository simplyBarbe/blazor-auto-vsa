using Server.Infrastructure.CRUD.Endpoints;
using Shared.Features.Products.Delete;

namespace Server.Features.Products.Delete;

public class DeleteProductEndpoint : DeleteEntityEndpointBase<int, DeleteProductCommand>
{
    protected override string GetRoute() => "/api/products/{key:int}";

    protected override DeleteProductCommand CreateCommand(int key)
    {
        return new DeleteProductCommand(key);
    }
}
