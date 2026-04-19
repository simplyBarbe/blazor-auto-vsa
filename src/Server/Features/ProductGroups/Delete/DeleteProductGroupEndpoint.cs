using Server.Infrastructure.CRUD.Endpoints;
using Shared.Features.ProductGroups.Delete;

namespace Server.Features.ProductGroups.Delete;

public class DeleteProductGroupEndpoint : DeleteEntityEndpointBase<int, DeleteProductGroupCommand>
{
    protected override string GetRoute() => "/api/groups/{key:int}";

    protected override DeleteProductGroupCommand CreateCommand(int key) => new(key);
}
