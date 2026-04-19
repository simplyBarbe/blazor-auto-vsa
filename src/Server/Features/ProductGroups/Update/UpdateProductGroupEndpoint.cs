using Server.Infrastructure.CRUD.Endpoints;
using Shared.Core;
using Shared.Features.ProductGroups.Responses;
using Shared.Features.ProductGroups.Update;

namespace Server.Features.ProductGroups.Update;

public class UpdateProductGroupEndpoint : UpdateEntityEndpointBase<int, UpdateProductGroupCommand, ProductGroupResponse>
{
    protected override string GetRoute() => "/api/groups/{key:int}";

    protected override async Task<IResult> HandleAsync(
        int key,
        UpdateProductGroupCommand command,
        IRequestSender sender,
        CancellationToken cancellationToken = default)
    {
        command.Id = key;
        return await base.HandleAsync(key, command, sender, cancellationToken);
    }
}
