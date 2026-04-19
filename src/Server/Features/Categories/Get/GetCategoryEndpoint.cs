using Server.Infrastructure.CRUD.Endpoints;
using Shared.Features.Categories.Get;
using Shared.Features.Categories.Responses;

namespace Server.Features.Categories.Get;

public class GetCategoryEndpoint : GetEntityEndpointBase<int, GetCategoryQuery, CategoryResponse>
{
    protected override string GetRoute() => "/api/categories/{key:int}";

    protected override GetCategoryQuery CreateQuery(int key) => new(key);
}
