using Shared.Core;
using Shared.Core.CRUD;

namespace Shared.Features.Products.Get;

/// <summary>
/// Query to retrieve a product by its ID.
/// </summary>
public class GetProductQuery : IRequest<Shared.Features.Products.Responses.ProductResponse>, IEntityKeyProvider
{
    /// <summary>
    /// The ID of the product to retrieve.
    /// </summary>
    public int Id { get; set; }

    /// <inheritdoc />
    public object[] GetKeys() => new object[] { Id };

    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public GetProductQuery() { }

    /// <summary>
    /// Constructor with ID parameter.
    /// </summary>
    public GetProductQuery(int id)
    {
        Id = id;
    }
}
