using System.Linq.Expressions;

namespace Server.Infrastructure.Data.Contracts;

/// <summary>
/// Represents the sort direction.
/// </summary>
public enum SortDirection
{
    /// <summary>
    /// Ascending sort order.
    /// </summary>
    Ascending,

    /// <summary>
    /// Descending sort order.
    /// </summary>
    Descending
}

/// <summary>
/// Represents a sort expression with direction.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class SortExpression<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets the sort key selector.
    /// </summary>
    public Expression<Func<TEntity, object>> KeySelector { get; }

    /// <summary>
    /// Gets the sort direction.
    /// </summary>
    public SortDirection Direction { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SortExpression{TEntity}"/> class.
    /// </summary>
    /// <param name="keySelector">The sort key selector.</param>
    /// <param name="direction">The sort direction. Default is ascending.</param>
    public SortExpression(Expression<Func<TEntity, object>> keySelector, SortDirection direction = SortDirection.Ascending)
    {
        KeySelector = keySelector;
        Direction = direction;
    }
}

/// <summary>
/// Represents query filter options including filtering, pagination, and sorting.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class QueryFilter<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets or sets the filter predicates. All predicates are combined with AND.
    /// </summary>
    public List<Expression<Func<TEntity, bool>>>? Filters { get; set; }

    /// <summary>
    /// Gets or sets the sort expressions with direction. Supports multiple columns sorting.
    /// </summary>
    public List<SortExpression<TEntity>>? OrderBy { get; set; }

    /// <summary>
    /// Gets or sets the number of items to skip (for pagination).
    /// </summary>
    public int? Skip { get; set; }

    /// <summary>
    /// Gets or sets the number of items to take (for pagination).
    /// </summary>
    public int? Take { get; set; }
}
