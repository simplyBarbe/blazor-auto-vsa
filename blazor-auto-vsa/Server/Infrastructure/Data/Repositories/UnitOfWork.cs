using AutoMapper;
using Server.Infrastructure.Data.Contracts;

namespace Server.Infrastructure.Data.Repositories;

/// <summary>
/// Unit of Work implementation for managing transactions and providing repository access.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly Dictionary<Type, object> _readRepositories = new();
    private readonly Dictionary<Type, object> _writeRepositories = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public UnitOfWork(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    /// <inheritdoc />
    public IReadRepository<TEntity> ReadRepository<TEntity>() where TEntity : class
    {
        var entityType = typeof(TEntity);

        if (_readRepositories.TryGetValue(entityType, out var repository))
        {
            return (IReadRepository<TEntity>)repository;
        }

        var newRepository = new ReadRepository<TEntity>(_context, _mapper);
        _readRepositories[entityType] = newRepository;
        return newRepository;
    }

    /// <inheritdoc />
    public IWriteRepository<TEntity> WriteRepository<TEntity>() where TEntity : class
    {
        var entityType = typeof(TEntity);

        if (_writeRepositories.TryGetValue(entityType, out var repository))
        {
            return (IWriteRepository<TEntity>)repository;
        }

        var newRepository = new WriteRepository<TEntity>(_context);
        _writeRepositories[entityType] = newRepository;
        return newRepository;
    }
}
