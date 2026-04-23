using Server.Infrastructure.CRUD.Endpoints;
using Shared.Features.Categories.Delete;

namespace Server.Features.Categories.Delete;

public class DeleteCategoryEndpoint : DeleteEntityEndpointBase<int, DeleteCategoryCommand>
{
    protected override string GetRoute() => "/api/categories/{key:int}";

    protected override DeleteCategoryCommand CreateCommand(int key) => new(key);
}
