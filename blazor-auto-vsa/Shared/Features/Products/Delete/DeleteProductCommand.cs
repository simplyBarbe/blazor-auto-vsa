using Shared.Core;
using Shared.Core.CRUD;

namespace Shared.Features.Products.Delete;

/// <summary>
/// Command to delete a product.
/// </summary>
public class DeleteProductCommand : IRequest<object?>, IEntityKeyProvider
{
    /// <summary>
    /// The ID of the product to delete.
    /// </summary>
    public int Id { get; set; }

    /// <inheritdoc />
    public object[] GetKeys() => new object[] { Id };

    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public DeleteProductCommand() { }

    /// <summary>
    /// Constructor with ID parameter.
    /// </summary>
    public DeleteProductCommand(int id)
    {
        Id = id;
    }
}
