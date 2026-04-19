using Shared.Core;
using Shared.Core.CRUD;
using Shared.Features.Categories.Responses;

namespace Shared.Features.Categories.Update;

public class UpdateCategoryCommand : IRequest<CategoryResponse>, IEntityKeyProvider
{
    public int Id { get; set; }
    public object[] GetKeys() => new object[] { Id };
    public string Name { get; set; } = string.Empty;

    public UpdateCategoryCommand() { }

    public UpdateCategoryCommand(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
