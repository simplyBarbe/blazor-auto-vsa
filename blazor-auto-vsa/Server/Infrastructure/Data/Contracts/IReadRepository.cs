namespace Server.Infrastructure.Data.Contracts;

/// <summary>
/// Generic read repository interface for querying entities.
/// Follows Interface Segregation Principle - only read operations.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IReadRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets an entity by its key. Supports composite keys.
    /// </summary>
    /// <param name="keyValues">The key values. For composite keys, provide values in the order defined in the entity configuration.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<TEntity?> GetByKeyAsync(params object[] keyValues);

    /// <summary>
    /// Gets entities projected to the specified type based on filter options.
    /// Uses AutoMapper ProjectTo for efficient database-level projection.
    /// Use TEntity as projection type to retrieve the domain model.
    /// </summary>
    /// <typeparam name="TProjection">The projection type.</typeparam>
    /// <param name="filter">The query filter options. Pass null to get all entities.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only list of projected entities matching the filter.</returns>
    Task<IReadOnlyList<TProjection>> GetAsync<TProjection>(QueryFilter<TEntity>? filter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of entities matching the optional filter.
    /// </summary>
    /// <param name="filter">The query filter options. Pass null to count all entities.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The count of matching entities.</returns>
    Task<int> CountAsync(QueryFilter<TEntity>? filter = null, CancellationToken cancellationToken = default);
}
