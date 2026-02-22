using AutoMapper;
using Server.Infrastructure.Data.Contracts;
using Shared.Core;
using Shared.Core.CRUD;
using Shared.Core.Exceptions;

namespace Server.Infrastructure.CRUD.Handlers;

/// <summary>Base handler for update. Supports simple and composite keys.</summary>
public abstract class UpdateEntityHandlerBase<TEntity, TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TEntity : class
    where TCommand : IRequest<TResponse>, IEntityKeyProvider
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly KeyExtractor _keyExtractor;

    protected UpdateEntityHandlerBase(IUnitOfWork unitOfWork, IMapper mapper, KeyExtractor? keyExtractor = null)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _keyExtractor = keyExtractor ?? KeyExtractor.Default;
    }

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

    protected virtual void UpdateEntity(TEntity entity, TCommand command) => _mapper.Map(command, entity);

    protected virtual TResponse MapToResponse(TEntity entity) => _mapper.Map<TResponse>(entity);
}
