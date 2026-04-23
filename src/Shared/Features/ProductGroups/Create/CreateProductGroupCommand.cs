using Shared.Core;
using Shared.Core.CRUD;
using Shared.Features.ProductGroups.Responses;

namespace Shared.Features.ProductGroups.Create;

public class CreateProductGroupCommand : IRequest<ProductGroupResponse>
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;

    public CreateProductGroupCommand() { }

    public CreateProductGroupCommand(int categoryId, string name)
    {
        CategoryId = categoryId;
        Name = name;
    }
}
