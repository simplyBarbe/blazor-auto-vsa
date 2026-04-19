using Microsoft.EntityFrameworkCore;
using Server.Domain;
using Server.Infrastructure.Data;
using Server.Infrastructure.Data.Contracts;
using Shared.Core;
using Shared.Features.Products.List;
using Shared.Features.Products.Responses;
using System.Linq.Expressions;

namespace Server.Features.Products.List;

public class ListProductHandler : IRequestHandler<ListProductQuery, PagedResult<ProductResponse>>
{
    private readonly ApplicationDbContext _context;

    public ListProductHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProductResponse>> Handle(ListProductQuery request, CancellationToken cancellationToken = default)
    {
        var filter = BuildQueryFilter(request);
        IQueryable<Product> query = _context.Products.AsNoTracking();

        if (filter.Filters is not null)
        {
            foreach (var predicate in filter.Filters)
            {
                query = query.Where(predicate);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, filter);
        query = ApplyPagination(query, filter);

        var items = await query
            .Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Price,
                p.GroupId,
                p.Group.CategoryId,
                p.Group.Category.Name,
                p.Group.Name))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductResponse>
        {
            Items = items,
            PageNumber = request.PageNumber ?? 1,
            PageSize = request.PageSize ?? 10,
            TotalCount = totalCount
        };
    }

    private static IQueryable<Product> ApplySorting(IQueryable<Product> query, QueryFilter<Product> filter)
    {
        if (filter.OrderBy is not { Count: > 0 })
        {
            return query;
        }

        IOrderedQueryable<Product>? orderedQuery = null;

        for (var i = 0; i < filter.OrderBy.Count; i++)
        {
            var sort = filter.OrderBy[i];

            if (i == 0)
            {
                orderedQuery = sort.Direction == SortDirection.Descending
                    ? query.OrderByDescending(sort.KeySelector)
                    : query.OrderBy(sort.KeySelector);
            }
            else
            {
                orderedQuery = sort.Direction == SortDirection.Descending
                    ? orderedQuery!.ThenByDescending(sort.KeySelector)
                    : orderedQuery!.ThenBy(sort.KeySelector);
            }
        }

        return orderedQuery!;
    }

    private static IQueryable<Product> ApplyPagination(IQueryable<Product> query, QueryFilter<Product> filter)
    {
        if (filter.Skip.HasValue)
        {
            query = query.Skip(filter.Skip.Value);
        }

        if (filter.Take.HasValue)
        {
            query = query.Take(filter.Take.Value);
        }

        return query;
    }

    private static QueryFilter<Product> BuildQueryFilter(ListProductQuery query)
    {
        var pageNumber = query.PageNumber ?? 1;
        var pageSize = query.PageSize ?? 10;

        var filter = new QueryFilter<Product>
        {
            Skip = (pageNumber - 1) * pageSize,
            Take = pageSize,
            OrderBy = new List<SortExpression<Product>> { BuildSort(query) }
        };

        var predicates = new List<Expression<Func<Product, bool>>>();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            predicates.Add(p => p.Name.Contains(query.SearchTerm!));
        }

        if (query.CategoryId is { } categoryId)
        {
            predicates.Add(p => p.Group.CategoryId == categoryId);
        }

        if (query.GroupId is { } groupId)
        {
            predicates.Add(p => p.GroupId == groupId);
        }

        if (predicates.Count > 0)
        {
            filter.Filters = predicates;
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
            "categoryname" => new SortExpression<Product>(p => p.Group.Category.Name, direction),
            "groupname" => new SortExpression<Product>(p => p.Group.Name, direction),
            _ => new SortExpression<Product>(p => p.Name, SortDirection.Ascending)
        };
    }
}
