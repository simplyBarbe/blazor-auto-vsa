using AutoMapper;
using Server.Infrastructure.Data.Contracts;
using Shared.Core;
using Shared.Core.CRUD;

namespace Server.Infrastructure.CRUD.Handlers;

/// <summary>
/// Abstract base handler for creating entities.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public abstract class CreateEntityHandlerBase<TEntity, TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TEntity : class
    where TCommand : CreateEntityCommand<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateEntityHandlerBase{TEntity, TCommand, TResponse}"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    protected CreateEntityHandlerBase(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(request);
        await _unitOfWork.WriteRepository<TEntity>().AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToResponse(entity);
    }

    /// <summary>
    /// Maps the command to an entity. Can be overridden for custom mapping logic.
    /// </summary>
    /// <param name="command">The command to map.</param>
    /// <returns>The mapped entity.</returns>
    protected virtual TEntity MapToEntity(TCommand command)
    {
        return _mapper.Map<TEntity>(command);
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
