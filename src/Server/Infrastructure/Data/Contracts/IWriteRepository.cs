namespace Server.Infrastructure.Data.Contracts;

public interface IWriteRepository<TEntity> where TEntity : class
{
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    Task DeleteAsync(params object[] keyValues);
}
