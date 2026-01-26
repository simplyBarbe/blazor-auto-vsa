using Server.Infrastructure.CRUD.Endpoints;
using Shared.Core;
using Shared.Features.Products.Update;
using Shared.Features.Products.Responses;

namespace Server.Features.Products.Update;

/// <summary>
/// Endpoint for updating a product.
/// </summary>
public class UpdateProductEndpoint : UpdateEntityEndpointBase<int, UpdateProductCommand, ProductResponse>
{
    /// <inheritdoc />
    protected override string GetRoute() => "/api/products/{key:int}";

    /// <inheritdoc />
    protected override async Task<IResult> HandleAsync(
        int key,
        UpdateProductCommand command,
        IRequestSender sender,
        CancellationToken cancellationToken = default)
    {
        // Set the Id from route parameter (KeyExtractor will use this "Id" property)
        command.Id = key;
        return await base.HandleAsync(key, command, sender, cancellationToken);
    }
}
