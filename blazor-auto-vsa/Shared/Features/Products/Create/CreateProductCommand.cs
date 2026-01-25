using Shared.Core;
using Shared.Core.CRUD;

namespace Shared.Features.Products.Create;

/// <summary>
/// Command to create a new product.
/// </summary>
public class CreateProductCommand : CreateEntityCommand<Shared.Features.Products.Responses.ProductResponse>
{
    /// <summary>
    /// The name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The price of the product.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public CreateProductCommand() { }

    /// <summary>
    /// Constructor with parameters.
    /// </summary>
    public CreateProductCommand(string name, decimal price)
    {
        Name = name;
        Price = price;
    }
}
