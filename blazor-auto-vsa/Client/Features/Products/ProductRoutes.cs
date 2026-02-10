using Client.Dispatching;
using Shared.Features.Products.Create;
using Shared.Features.Products.Delete;
using Shared.Features.Products.Get;
using Shared.Features.Products.List;
using Shared.Features.Products.Update;

namespace Client.Features.Products;

public class ProductRoutes : IRouteDefinition
{
    public void Define(IRouteMap map)
    {
        map.Map<GetProductQuery>("/api/products/{Id:guid}", HttpMethod.Get);
        map.Map<ListProductQuery>("/api/products", HttpMethod.Get);
        map.Map<CreateProductCommand>("/api/products", HttpMethod.Post);
        map.Map<UpdateProductCommand>("/api/products/{Id:guid}", HttpMethod.Put);
        map.Map<DeleteProductCommand>("/api/products/{Id:guid}", HttpMethod.Delete);
    }
}
