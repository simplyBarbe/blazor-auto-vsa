using Server.Infrastructure.Data.Contracts;
using Shared.Core;
using Shared.Core.CRUD;
using Shared.Core.Exceptions;

namespace Server.Infrastructure.CRUD.Handlers;

/// <summary>Base handler for delete. Supports simple and composite keys.</summary>
public abstract class DeleteEntityHandlerBase<TEntity, TCommand> : IRequestHandler<TCommand, object?>
    where TEntity : class
    where TCommand : IRequest<object?>, IEntityKeyProvider
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly KeyExtractor _keyExtractor;

    protected DeleteEntityHandlerBase(IUnitOfWork unitOfWork, KeyExtractor? keyExtractor = null)
    {
        _unitOfWork = unitOfWork;
        _keyExtractor = keyExtractor ?? KeyExtractor.Default;
    }

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
