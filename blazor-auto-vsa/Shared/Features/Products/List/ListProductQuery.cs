using Shared.Core;
using Shared.Core.CRUD;

namespace Shared.Features.Products.List;

/// <summary>
/// Query to retrieve a list of products with pagination and filtering.
/// </summary>
public class ListProductQuery : IRequest<PagedResult<Shared.Features.Products.Responses.ProductResponse>>, IPageableQuery
{
    /// <summary>
    /// Gets or sets the page number (1-based).
    /// </summary>
    public int? PageNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    public int? PageSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets the search term to filter products by name.
    /// </summary>
    public string? SearchTerm { get; set; }
}
