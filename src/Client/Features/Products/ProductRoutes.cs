using Client.Dispatching;
using Shared.Features.Products.Create;
using Shared.Features.Products.Delete;
using Shared.Features.Products.Get;
using Shared.Features.Products.List;
using Shared.Features.Products.Update;

namespace Client.Features.Products;

public sealed class ProductRoutes : IRouteDefinition
{
    public void Define(RequestEndpointMapper routes)
    {
        routes.Map<GetProductQuery>("/api/products/{Id}", HttpMethod.Get);
        routes.Map<ListProductQuery>("/api/products", HttpMethod.Get);
        routes.Map<CreateProductCommand>("/api/products", HttpMethod.Post);
        routes.Map<UpdateProductCommand>("/api/products/{Id}", HttpMethod.Put);
        routes.Map<DeleteProductCommand>("/api/products/{Id}", HttpMethod.Delete);
    }
}
