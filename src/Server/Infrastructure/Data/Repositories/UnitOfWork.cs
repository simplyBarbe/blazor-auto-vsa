using AutoMapper;
using Server.Infrastructure.Data.Contracts;

namespace Server.Infrastructure.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly Dictionary<Type, object> _readRepositories = new();
    private readonly Dictionary<Type, object> _writeRepositories = new();

    public UnitOfWork(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

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
