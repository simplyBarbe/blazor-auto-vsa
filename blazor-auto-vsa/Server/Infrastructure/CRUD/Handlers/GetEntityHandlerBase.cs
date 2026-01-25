using Server.Infrastructure.Data.Contracts;
using Shared.Core;
using Shared.Core.CRUD;
using Shared.Core.Exceptions;
using AutoMapper;

namespace Server.Infrastructure.CRUD.Handlers;

/// <summary>
/// Abstract base handler for getting an entity by key.
/// Supports simple types (int, Guid, string) and composite keys (tuples, records).
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public abstract class GetEntityHandlerBase<TEntity, TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TEntity : class
    where TQuery : GetEntityQuery<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly KeyExtractor _keyExtractor;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetEntityHandlerBase{TEntity, TQuery, TResponse}"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="keyExtractor">The key extractor. If null, uses the default instance.</param>
    protected GetEntityHandlerBase(IUnitOfWork unitOfWork, IMapper mapper, KeyExtractor? keyExtractor = null)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _keyExtractor = keyExtractor ?? KeyExtractor.Default;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(TQuery request, CancellationToken cancellationToken = default)
    {
        var keyValues = _keyExtractor.GetKeyValues(request);
        var entity = await _unitOfWork.ReadRepository<TEntity>().GetByKeyAsync(keyValues);

        if (entity is null)
        {
            throw new EntityNotFoundException(typeof(TEntity).Name, keyValues);
        }

        return MapToResponse(entity);
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
