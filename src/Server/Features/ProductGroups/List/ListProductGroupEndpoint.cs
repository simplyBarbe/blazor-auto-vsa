using Server.Infrastructure.CRUD.Endpoints;
using Shared.Features.ProductGroups.List;
using Shared.Features.ProductGroups.Responses;

namespace Server.Features.ProductGroups.List;

public class ListProductGroupEndpoint : ListEntityEndpointBase<ListProductGroupQuery, ProductGroupResponse>
{
    protected override string GetRoute() => "/api/groups";
}
