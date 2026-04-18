using AutoMapper;
using Server.Domain;
using Server.Infrastructure.CRUD.Handlers;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.Products.List;
using Shared.Features.Products.Responses;
using System.Linq.Expressions;

namespace Server.Features.Products.List;

public class ListProductHandler : ListEntityHandlerBase<Product, ListProductQuery, ProductResponse>
{
    public ListProductHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper)
    {
    }

    protected override QueryFilter<Product> BuildQueryFilter(ListProductQuery query)
    {
        var pageNumber = query.PageNumber ?? 1;
        var pageSize = query.PageSize ?? 10;

        var filter = new QueryFilter<Product>
        {
            Skip = (pageNumber - 1) * pageSize,
            Take = pageSize,
            OrderBy = new List<SortExpression<Product>> { BuildSort(query) }
        };

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            filter.Filters = new List<Expression<Func<Product, bool>>>
            {
                p => p.Name.Contains(query.SearchTerm)
            };
        }

        return filter;
    }

    private static SortExpression<Product> BuildSort(ListProductQuery query)
    {
        var field = query.SortBy?.Trim();
        var ascending = query.SortAscending ?? true;
        var direction = ascending ? SortDirection.Ascending : SortDirection.Descending;

        return field?.ToLowerInvariant() switch
        {
            "id" => new SortExpression<Product>(p => p.Id, direction),
            "name" => new SortExpression<Product>(p => p.Name, direction),
            "price" => new SortExpression<Product>(p => p.Price, direction),
            _ => new SortExpression<Product>(p => p.Name, SortDirection.Ascending)
        };
    }
}
