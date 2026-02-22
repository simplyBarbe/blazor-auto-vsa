namespace Server.Infrastructure.Data.Contracts;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
    IReadRepository<TEntity> ReadRepository<TEntity>() where TEntity : class;
    IWriteRepository<TEntity> WriteRepository<TEntity>() where TEntity : class;
}
