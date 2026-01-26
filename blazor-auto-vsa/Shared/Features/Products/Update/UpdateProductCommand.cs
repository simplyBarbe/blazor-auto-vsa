using Shared.Core;
using Shared.Core.CRUD;

namespace Shared.Features.Products.Update;

/// <summary>
/// Command to update an existing product.
/// </summary>
public class UpdateProductCommand : IRequest<Shared.Features.Products.Responses.ProductResponse>, IEntityKeyProvider
{
    /// <summary>
    /// The ID of the product to update.
    /// </summary>
    public int Id { get; set; }

    /// <inheritdoc />
    public object[] GetKeys() => new object[] { Id };

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price of the product.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public UpdateProductCommand() { }

    /// <summary>
    /// Constructor with parameters.
    /// </summary>
    public UpdateProductCommand(int id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }
}
