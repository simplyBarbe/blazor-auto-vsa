using Server.Infrastructure.CRUD.Endpoints;
using Shared.Features.ProductGroups.Get;
using Shared.Features.ProductGroups.Responses;

namespace Server.Features.ProductGroups.Get;

public class GetProductGroupEndpoint : GetEntityEndpointBase<int, GetProductGroupQuery, ProductGroupResponse>
{
    protected override string GetRoute() => "/api/groups/{key:int}";

    protected override GetProductGroupQuery CreateQuery(int key) => new(key);
}
