namespace Server.Infrastructure.Data.Contracts;

/// <summary>
/// Generic write repository interface for modifying entities.
/// Follows Interface Segregation Principle - only write operations.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IWriteRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The added entity.</returns>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an entity as modified.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    void Update(TEntity entity);

    /// <summary>
    /// Marks an entity as deleted.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    void Delete(TEntity entity);

    /// <summary>
    /// Finds and deletes an entity by its identifier. Supports composite keys.
    /// </summary>
    /// <param name="keyValues">The key values. For composite keys, provide values in the order defined in the entity configuration.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(params object[] keyValues);
}
