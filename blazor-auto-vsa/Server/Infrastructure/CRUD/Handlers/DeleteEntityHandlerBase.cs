using Server.Infrastructure.Data.Contracts;
using Shared.Core;
using Shared.Core.CRUD;
using Shared.Core.Exceptions;

namespace Server.Infrastructure.CRUD.Handlers;

/// <summary>
/// Abstract base handler for deleting entities.
/// Supports simple types (int, Guid, string) and composite keys (tuples, records).
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TCommand">The command type.</typeparam>
public abstract class DeleteEntityHandlerBase<TEntity, TCommand> : IRequestHandler<TCommand, object?>
    where TEntity : class
    where TCommand : DeleteEntityCommand
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly KeyExtractor _keyExtractor;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteEntityHandlerBase{TEntity, TCommand}"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="keyExtractor">The key extractor. If null, uses the default instance.</param>
    protected DeleteEntityHandlerBase(IUnitOfWork unitOfWork, KeyExtractor? keyExtractor = null)
    {
        _unitOfWork = unitOfWork;
        _keyExtractor = keyExtractor ?? KeyExtractor.Default;
    }

    /// <inheritdoc />
    public async Task<object?> Handle(TCommand request, CancellationToken cancellationToken = default)
    {
        var keyValues = _keyExtractor.GetKeyValues(request);
        var entity = await _unitOfWork.ReadRepository<TEntity>().GetByKeyAsync(keyValues);

        if (entity is null)
        {
            throw new EntityNotFoundException(typeof(TEntity).Name, keyValues);
        }

        _unitOfWork.WriteRepository<TEntity>().Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return null;
    }
}
