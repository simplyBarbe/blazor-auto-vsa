using Shared.Core;
using Shared.Core.CRUD;

namespace Shared.Features.ProductGroups.Delete;

public class DeleteProductGroupCommand : IRequest<object?>, IEntityKeyProvider
{
    public int Id { get; set; }
    public object[] GetKeys() => new object[] { Id };

    public DeleteProductGroupCommand() { }

    public DeleteProductGroupCommand(int id) => Id = id;
}
