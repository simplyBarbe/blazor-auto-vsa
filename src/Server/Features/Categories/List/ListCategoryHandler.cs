using AutoMapper;
using Server.Domain;
using Server.Infrastructure.CRUD.Handlers;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.Categories.List;
using Shared.Features.Categories.Responses;
using System.Linq.Expressions;

namespace Server.Features.Categories.List;

public class ListCategoryHandler : ListEntityHandlerBase<Category, ListCategoryQuery, CategoryResponse>
{
    public ListCategoryHandler(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
    {
    }

    protected override QueryFilter<Category> BuildQueryFilter(ListCategoryQuery query)
    {
        var pageNumber = query.PageNumber ?? 1;
        var pageSize = query.PageSize ?? 10;

        var filter = new QueryFilter<Category>
        {
            Skip = (pageNumber - 1) * pageSize,
            Take = pageSize,
            OrderBy = new List<SortExpression<Category>> { BuildSort(query) }
        };

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            filter.Filters = new List<Expression<Func<Category, bool>>>
            {
                c => c.Name.Contains(query.SearchTerm!)
            };
        }

        return filter;
    }

    private static SortExpression<Category> BuildSort(ListCategoryQuery query)
    {
        var field = query.SortBy?.Trim();
        var ascending = query.SortAscending ?? true;
        var direction = ascending ? SortDirection.Ascending : SortDirection.Descending;

        return field?.ToLowerInvariant() switch
        {
            "id" => new SortExpression<Category>(c => c.Id, direction),
            "name" => new SortExpression<Category>(c => c.Name, direction),
            _ => new SortExpression<Category>(c => c.Name, SortDirection.Ascending)
        };
    }
}
