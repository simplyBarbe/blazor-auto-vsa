using Shared.Core;
using Shared.Core.CRUD;

namespace Shared.Features.Categories.Delete;

public class DeleteCategoryCommand : IRequest<object?>, IEntityKeyProvider
{
    public int Id { get; set; }
    public object[] GetKeys() => new object[] { Id };

    public DeleteCategoryCommand() { }

    public DeleteCategoryCommand(int id) => Id = id;
}
