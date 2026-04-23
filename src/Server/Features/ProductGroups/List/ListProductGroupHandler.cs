using AutoMapper;
using Server.Domain;
using Server.Infrastructure.CRUD.Handlers;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.ProductGroups.List;
using Shared.Features.ProductGroups.Responses;
using System.Linq.Expressions;

namespace Server.Features.ProductGroups.List;

public class ListProductGroupHandler : ListEntityHandlerBase<ProductGroup, ListProductGroupQuery, ProductGroupResponse>
{
    public ListProductGroupHandler(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
    {
    }

    protected override QueryFilter<ProductGroup> BuildQueryFilter(ListProductGroupQuery query)
    {
        var pageNumber = query.PageNumber ?? 1;
        var pageSize = query.PageSize ?? 10;

        var filter = new QueryFilter<ProductGroup>
        {
            Skip = (pageNumber - 1) * pageSize,
            Take = pageSize,
            OrderBy = new List<SortExpression<ProductGroup>> { BuildSort(query) }
        };

        var predicates = new List<Expression<Func<ProductGroup, bool>>>();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            predicates.Add(g => g.Name.Contains(query.SearchTerm!));
        }

        if (query.CategoryId is { } categoryId)
        {
            predicates.Add(g => g.CategoryId == categoryId);
        }

        if (predicates.Count > 0)
        {
            filter.Filters = predicates;
        }

        return filter;
    }

    private static SortExpression<ProductGroup> BuildSort(ListProductGroupQuery query)
    {
        var field = query.SortBy?.Trim();
        var ascending = query.SortAscending ?? true;
        var direction = ascending ? SortDirection.Ascending : SortDirection.Descending;

        return field?.ToLowerInvariant() switch
        {
            "id" => new SortExpression<ProductGroup>(g => g.Id, direction),
            "categoryid" => new SortExpression<ProductGroup>(g => g.CategoryId, direction),
            "categoryname" => new SortExpression<ProductGroup>(g => g.Category.Name, direction),
            "name" => new SortExpression<ProductGroup>(g => g.Name, direction),
            _ => new SortExpression<ProductGroup>(g => g.Name, SortDirection.Ascending)
        };
    }
}
