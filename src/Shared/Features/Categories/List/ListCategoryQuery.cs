using Shared.Core;
using Shared.Core.CRUD;
using Shared.Features.Categories.Responses;

namespace Shared.Features.Categories.List;

public class ListCategoryQuery : IRequest<PagedResult<CategoryResponse>>, IPageableQuery
{
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool? SortAscending { get; set; }
}
