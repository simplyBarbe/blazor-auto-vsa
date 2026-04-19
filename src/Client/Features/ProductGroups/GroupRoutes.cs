using Client.Dispatching;
using Shared.Features.ProductGroups.Create;
using Shared.Features.ProductGroups.Delete;
using Shared.Features.ProductGroups.Get;
using Shared.Features.ProductGroups.List;
using Shared.Features.ProductGroups.Update;

namespace Client.Features.ProductGroups;

public sealed class GroupRoutes : IRouteDefinition
{
    public void Define(RequestEndpointMapper routes)
    {
        routes.Map<GetProductGroupQuery>("/api/groups/{Id}", HttpMethod.Get);
        routes.Map<ListProductGroupQuery>("/api/groups", HttpMethod.Get);
        routes.Map<CreateProductGroupCommand>("/api/groups", HttpMethod.Post);
        routes.Map<UpdateProductGroupCommand>("/api/groups/{Id}", HttpMethod.Put);
        routes.Map<DeleteProductGroupCommand>("/api/groups/{Id}", HttpMethod.Delete);
    }
}
