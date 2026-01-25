using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Server.Infrastructure.Data.Contracts;

namespace Server.Infrastructure.Data.Repositories;

/// <summary>
/// Generic read repository implementation with query optimizations.
/// Handles all read operations for any entity type.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class ReadRepository<TEntity> : IReadRepository<TEntity> where TEntity : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<TEntity> _dbSet;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadRepository{TEntity}"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public ReadRepository(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetByKeyAsync(params object[] keyValues)
    {
        return await _dbSet.FindAsync(keyValues);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TProjection>> GetAsync<TProjection>(QueryFilter<TEntity>? filter = null, CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(filter);
        return await query
            .ProjectTo<TProjection>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(QueryFilter<TEntity>? filter = null, CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(filter, applyPagination: false, applySorting: false);
        return await query.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Builds the query based on filter options.
    /// </summary>
    private IQueryable<TEntity> BuildQuery(QueryFilter<TEntity>? filter, bool applyPagination = true, bool applySorting = true)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        if (filter is null)
        {
            return query;
        }

        // Apply filter predicates (combined with AND)
        if (filter.Filters is not null)
        {
            foreach (var predicate in filter.Filters)
            {
                query = query.Where(predicate);
            }
        }

        // Apply sorting (supports multiple columns)
        if (applySorting && filter.OrderBy is { Count: > 0 })
        {
            IOrderedQueryable<TEntity>? orderedQuery = null;

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

            query = orderedQuery!;
        }

        // Apply pagination
        if (applyPagination)
        {
            if (filter.Skip.HasValue)
            {
                query = query.Skip(filter.Skip.Value);
            }

            if (filter.Take.HasValue)
            {
                query = query.Take(filter.Take.Value);
            }
        }

        return query;
    }
}
