using Client.Dispatching;
using Shared.Features.Categories.Create;
using Shared.Features.Categories.Delete;
using Shared.Features.Categories.Get;
using Shared.Features.Categories.List;
using Shared.Features.Categories.Update;

namespace Client.Features.Categories;

public sealed class CategoryRoutes : IRouteDefinition
{
    public void Define(RequestEndpointMapper routes)
    {
        routes.Map<GetCategoryQuery>("/api/categories/{Id}", HttpMethod.Get);
        routes.Map<ListCategoryQuery>("/api/categories", HttpMethod.Get);
        routes.Map<CreateCategoryCommand>("/api/categories", HttpMethod.Post);
        routes.Map<UpdateCategoryCommand>("/api/categories/{Id}", HttpMethod.Put);
        routes.Map<DeleteCategoryCommand>("/api/categories/{Id}", HttpMethod.Delete);
    }
}
