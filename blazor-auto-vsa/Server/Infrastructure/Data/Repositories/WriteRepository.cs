using Microsoft.EntityFrameworkCore;
using Server.Infrastructure.Data.Contracts;

namespace Server.Infrastructure.Data.Repositories;

/// <summary>
/// Generic write repository implementation.
/// Handles all write operations for any entity type.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class WriteRepository<TEntity> : IWriteRepository<TEntity> where TEntity : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="WriteRepository{TEntity}"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public WriteRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    /// <inheritdoc />
    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var entry = await _dbSet.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    /// <inheritdoc />
    public void Update(TEntity entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
    }

    /// <inheritdoc />
    public void Delete(TEntity entity)
    {
        _context.Entry(entity).State = EntityState.Deleted;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(params object[] keyValues)
    {
        var entity = await _dbSet.FindAsync(keyValues);

        if (entity is not null)
        {
            _context.Entry(entity).State = EntityState.Deleted;
        }
    }
}
