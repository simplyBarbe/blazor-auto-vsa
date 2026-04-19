using Server.Infrastructure.CRUD.Endpoints;
using Shared.Features.Categories.Create;
using Shared.Features.Categories.Responses;

namespace Server.Features.Categories.Create;

public class CreateCategoryEndpoint : CreateEntityEndpointBase<CreateCategoryCommand, CategoryResponse>
{
    protected override string GetRoute() => "/api/categories";

    protected override string GetCreatedLocation(object result)
    {
        if (result is CategoryResponse r)
        {
            return $"/api/categories/{r.Id}";
        }

        return base.GetCreatedLocation(result);
    }
}
