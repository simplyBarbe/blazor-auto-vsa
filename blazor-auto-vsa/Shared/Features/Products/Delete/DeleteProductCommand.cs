using Shared.Core.CRUD;

namespace Shared.Features.Products.Delete;

/// <summary>
/// Command to delete a product.
/// </summary>
public class DeleteProductCommand : DeleteEntityCommand
{
    /// <summary>
    /// The ID of the product to delete.
    /// KeyExtractor will automatically use this "Id" property.
    /// </summary>
    public int Id { get; set; }

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
