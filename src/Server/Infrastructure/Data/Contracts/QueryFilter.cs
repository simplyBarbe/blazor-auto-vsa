using System.Linq.Expressions;

namespace Server.Infrastructure.Data.Contracts;

public enum SortDirection
{
    Ascending,
    Descending
}

public class SortExpression<TEntity> where TEntity : class
{
    public Expression<Func<TEntity, object>> KeySelector { get; }
    public SortDirection Direction { get; }

    public SortExpression(Expression<Func<TEntity, object>> keySelector, SortDirection direction = SortDirection.Ascending)
    {
        KeySelector = keySelector;
        Direction = direction;
    }
}

public class QueryFilter<TEntity> where TEntity : class
{
    public List<Expression<Func<TEntity, bool>>>? Filters { get; set; }
    public List<SortExpression<TEntity>>? OrderBy { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
}
