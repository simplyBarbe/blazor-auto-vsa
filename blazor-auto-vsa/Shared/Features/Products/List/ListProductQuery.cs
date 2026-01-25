using Shared.Core.CRUD;

namespace Shared.Features.Products.List;

/// <summary>
/// Query to retrieve a list of products with pagination and filtering.
/// </summary>
public class ListProductQuery : ListEntityQuery<Shared.Features.Products.Responses.ProductResponse>
{
    /// <summary>
    /// Gets or sets the search term to filter products by name.
    /// </summary>
    public string? SearchTerm { get; set; }
}
