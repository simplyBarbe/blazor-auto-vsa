using Shared.Core;
using Shared.Core.CRUD;

namespace Shared.Features.Products.Delete;

public class DeleteProductCommand : IRequest<object?>, IEntityKeyProvider
{
    public int Id { get; set; }
    public object[] GetKeys() => new object[] { Id };

    public DeleteProductCommand() { }

    public DeleteProductCommand(int id)
    {
        Id = id;
    }
}
