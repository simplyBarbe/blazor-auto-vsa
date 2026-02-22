using AutoMapper;
using Server.Infrastructure.Data.Contracts;
using Shared.Core;
using Shared.Core.CRUD;

namespace Server.Infrastructure.CRUD.Handlers;

public abstract class ListEntityHandlerBase<TEntity, TQuery, TResponse> : IRequestHandler<TQuery, PagedResult<TResponse>>
    where TEntity : class
    where TQuery : IRequest<PagedResult<TResponse>>, IPageableQuery
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    protected ListEntityHandlerBase(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<TResponse>> Handle(TQuery request, CancellationToken cancellationToken = default)
    {
        var filter = BuildQueryFilter(request);
        var items = await _unitOfWork.ReadRepository<TEntity>().GetAsync<TResponse>(filter, cancellationToken);
        var totalCount = await _unitOfWork.ReadRepository<TEntity>().CountAsync(filter, cancellationToken);

        return new PagedResult<TResponse>
        {
            Items = items,
            PageNumber = request.PageNumber ?? 1,
            PageSize = request.PageSize ?? 10,
            TotalCount = totalCount
        };
    }

    protected abstract QueryFilter<TEntity> BuildQueryFilter(TQuery query);

    protected virtual TResponse MapToResponse(TEntity entity) => _mapper.Map<TResponse>(entity);
}
