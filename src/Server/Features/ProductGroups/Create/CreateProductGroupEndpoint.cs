using Server.Infrastructure.CRUD.Endpoints;
using Shared.Features.ProductGroups.Create;
using Shared.Features.ProductGroups.Responses;

namespace Server.Features.ProductGroups.Create;

public class CreateProductGroupEndpoint : CreateEntityEndpointBase<CreateProductGroupCommand, ProductGroupResponse>
{
    protected override string GetRoute() => "/api/groups";

    protected override string GetCreatedLocation(object result)
    {
        if (result is ProductGroupResponse r)
        {
            return $"/api/groups/{r.Id}";
        }

        return base.GetCreatedLocation(result);
    }
}
