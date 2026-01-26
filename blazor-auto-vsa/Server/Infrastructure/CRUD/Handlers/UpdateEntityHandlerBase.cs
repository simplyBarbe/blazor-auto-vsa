using AutoMapper;
using Server.Infrastructure.Data.Contracts;
using Shared.Core;
using Shared.Core.CRUD;
using Shared.Core.Exceptions;

namespace Server.Infrastructure.CRUD.Handlers;

/// <summary>
/// Abstract base handler for updating entities.
/// Supports simple types (int, Guid, string) and composite keys (tuples, records).
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public abstract class UpdateEntityHandlerBase<TEntity, TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TEntity : class
    where TCommand : IRequest<TResponse>, IEntityKeyProvider
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly KeyExtractor _keyExtractor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateEntityHandlerBase{TEntity, TCommand, TResponse}"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="keyExtractor">The key extractor. If null, uses the default instance.</param>
    protected UpdateEntityHandlerBase(IUnitOfWork unitOfWork, IMapper mapper, KeyExtractor? keyExtractor = null)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _keyExtractor = keyExtractor ?? KeyExtractor.Default;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken = default)
    {
        var keyValues = _keyExtractor.GetKeyValues(request);
        var entity = await _unitOfWork.ReadRepository<TEntity>().GetByKeyAsync(keyValues);

        if (entity is null)
        {
            throw new EntityNotFoundException(typeof(TEntity).Name, keyValues);
        }

        UpdateEntity(entity, request);
        _unitOfWork.WriteRepository<TEntity>().Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(entity);
    }

    /// <summary>
    /// Updates the entity with values from the command. Can be overridden for custom update logic.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="command">The command containing update values.</param>
    protected virtual void UpdateEntity(TEntity entity, TCommand command)
    {
        _mapper.Map(command, entity);
    }

    /// <summary>
    /// Maps the entity to a response. Can be overridden for custom mapping logic.
    /// </summary>
    /// <param name="entity">The entity to map.</param>
    /// <returns>The mapped response.</returns>
    protected virtual TResponse MapToResponse(TEntity entity)
    {
        return _mapper.Map<TResponse>(entity);
    }
}
