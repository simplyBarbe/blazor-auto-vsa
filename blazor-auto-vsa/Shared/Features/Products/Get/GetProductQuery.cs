using Shared.Core;

namespace Shared.Features.Products.Get;

/// <summary>
/// Query to retrieve a product by its ID.
/// </summary>
public class GetProductQuery : IRequest<Shared.Features.Products.Responses.ProductResponse>
{
    /// <summary>
    /// The ID of the product to retrieve.
    /// </summary>
    public int Id { get; set; }

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
