using Shared.Core;
using Shared.Core.CRUD;
using Shared.Features.ProductGroups.Responses;

namespace Shared.Features.ProductGroups.Update;

public class UpdateProductGroupCommand : IRequest<ProductGroupResponse>, IEntityKeyProvider
{
    public int Id { get; set; }
    public object[] GetKeys() => new object[] { Id };
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;

    public UpdateProductGroupCommand() { }

    public UpdateProductGroupCommand(int id, int categoryId, string name)
    {
        Id = id;
        CategoryId = categoryId;
        Name = name;
    }
}
