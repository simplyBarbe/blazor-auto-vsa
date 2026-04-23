using Shared.Core;
using Shared.Core.CRUD;
using Shared.Features.ProductGroups.Responses;

namespace Shared.Features.ProductGroups.List;

public class ListProductGroupQuery : IRequest<PagedResult<ProductGroupResponse>>, IPageableQuery
{
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }

    /// <summary>When set, only groups in this category are returned.</summary>
    public int? CategoryId { get; set; }

    public string? SortBy { get; set; }
    public bool? SortAscending { get; set; }
}
