using Shared.Core;
using Shared.Core.CRUD;

namespace Shared.Features.Products.List;

public class ListProductQuery : IRequest<PagedResult<Shared.Features.Products.Responses.ProductResponse>>, IPageableQuery
{
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }

    /// <summary>Sort field: Id, Name, or Price (from FluentDataGrid column property names).</summary>
    public string? SortBy { get; set; }

    public bool? SortAscending { get; set; }
}
