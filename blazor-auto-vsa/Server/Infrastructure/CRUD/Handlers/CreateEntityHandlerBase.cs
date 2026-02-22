using AutoMapper;
using Server.Infrastructure.Data.Contracts;
using Shared.Core;
using Shared.Core.CRUD;

namespace Server.Infrastructure.CRUD.Handlers;

public abstract class CreateEntityHandlerBase<TEntity, TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TEntity : class
    where TCommand : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    protected CreateEntityHandlerBase(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(request);
        await _unitOfWork.WriteRepository<TEntity>().AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToResponse(entity);
    }

    protected virtual TEntity MapToEntity(TCommand command) => _mapper.Map<TEntity>(command);

    protected virtual TResponse MapToResponse(TEntity entity)
    {
        return _mapper.Map<TResponse>(entity);
    }
}
