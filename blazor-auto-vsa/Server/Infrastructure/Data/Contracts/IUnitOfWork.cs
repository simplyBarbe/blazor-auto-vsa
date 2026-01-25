namespace Server.Infrastructure.Data.Contracts;

/// <summary>
/// Unit of Work interface for managing transactions and providing repository access.
/// Follows Single Responsibility Principle - only manages transactions and provides repository access.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in this unit of work to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes made in this unit of work to the database.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    int SaveChanges();

    /// <summary>
    /// Gets a read repository for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>A read repository for the entity type.</returns>
    IReadRepository<TEntity> ReadRepository<TEntity>() where TEntity : class;

    /// <summary>
    /// Gets a write repository for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>A write repository for the entity type.</returns>
    IWriteRepository<TEntity> WriteRepository<TEntity>() where TEntity : class;
}
