using AutoMapper;
using Server.Domain;
using Server.Infrastructure.CRUD.Handlers;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.Products.List;
using Shared.Features.Products.Responses;
using System.Linq.Expressions;

namespace Server.Features.Products.List;

/// <summary>
/// Handler for ListProductQuery - retrieves a list of products with pagination and filtering.
/// </summary>
public class ListProductHandler : ListEntityHandlerBase<Product, ListProductQuery, ProductResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListProductHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public ListProductHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper)
    {
    }

    /// <inheritdoc />
    protected override QueryFilter<Product> BuildQueryFilter(ListProductQuery query)
    {
        var pageNumber = query.PageNumber ?? 1;
        var pageSize = query.PageSize ?? 10;

        var filter = new QueryFilter<Product>
        {
            Skip = (pageNumber - 1) * pageSize,
            Take = pageSize,
            OrderBy = new List<SortExpression<Product>>
            {
                new(p => p.Name, SortDirection.Ascending)
            }
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
}
