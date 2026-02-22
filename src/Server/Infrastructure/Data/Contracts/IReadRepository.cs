namespace Server.Infrastructure.Data.Contracts;

public interface IReadRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByKeyAsync(params object[] keyValues);
    Task<IReadOnlyList<TProjection>> GetAsync<TProjection>(QueryFilter<TEntity>? filter = null, CancellationToken cancellationToken = default);
    Task<int> CountAsync(QueryFilter<TEntity>? filter = null, CancellationToken cancellationToken = default);
}
