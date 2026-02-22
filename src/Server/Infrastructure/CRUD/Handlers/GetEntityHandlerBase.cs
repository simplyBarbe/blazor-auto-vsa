using Server.Infrastructure.Data.Contracts;
using Shared.Core;
using Shared.Core.CRUD;
using Shared.Core.Exceptions;
using AutoMapper;

namespace Server.Infrastructure.CRUD.Handlers;

/// <summary>Base handler for get-by-key. Supports simple keys and composite keys (tuples, records).</summary>
public abstract class GetEntityHandlerBase<TEntity, TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TEntity : class
    where TQuery : IRequest<TResponse>, IEntityKeyProvider
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly KeyExtractor _keyExtractor;

    protected GetEntityHandlerBase(IUnitOfWork unitOfWork, IMapper mapper, KeyExtractor? keyExtractor = null)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _keyExtractor = keyExtractor ?? KeyExtractor.Default;
    }

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

    protected virtual TResponse MapToResponse(TEntity entity)
    {
        return _mapper.Map<TResponse>(entity);
    }
}
