using Server.Infrastructure.CRUD.Endpoints;
using Shared.Features.Categories.List;
using Shared.Features.Categories.Responses;

namespace Server.Features.Categories.List;

public class ListCategoryEndpoint : ListEntityEndpointBase<ListCategoryQuery, CategoryResponse>
{
    protected override string GetRoute() => "/api/categories";
}
