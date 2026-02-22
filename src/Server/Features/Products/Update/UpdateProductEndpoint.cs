using Server.Infrastructure.CRUD.Endpoints;
using Shared.Core;
using Shared.Features.Products.Update;
using Shared.Features.Products.Responses;

namespace Server.Features.Products.Update;

public class UpdateProductEndpoint : UpdateEntityEndpointBase<int, UpdateProductCommand, ProductResponse>
{
    protected override string GetRoute() => "/api/products/{key:int}";

    protected override async Task<IResult> HandleAsync(
        int key,
        UpdateProductCommand command,
        IRequestSender sender,
        CancellationToken cancellationToken = default)
    {
        command.Id = key;
        return await base.HandleAsync(key, command, sender, cancellationToken);
    }
}
