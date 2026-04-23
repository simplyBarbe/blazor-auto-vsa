using Server.Infrastructure.CRUD.Endpoints;
using Shared.Core;
using Shared.Features.Categories.Responses;
using Shared.Features.Categories.Update;

namespace Server.Features.Categories.Update;

public class UpdateCategoryEndpoint : UpdateEntityEndpointBase<int, UpdateCategoryCommand, CategoryResponse>
{
    protected override string GetRoute() => "/api/categories/{key:int}";

    protected override async Task<IResult> HandleAsync(
        int key,
        UpdateCategoryCommand command,
        IRequestSender sender,
        CancellationToken cancellationToken = default)
    {
        command.Id = key;
        return await base.HandleAsync(key, command, sender, cancellationToken);
    }
}
