using AutoMapper;
using Server.Infrastructure.Data.Contracts;
using Shared.Core;
using Shared.Core.CRUD;

namespace Server.Infrastructure.CRUD.Handlers;

/// <summary>
/// Abstract base handler for listing entities with pagination and filtering.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public abstract class ListEntityHandlerBase<TEntity, TQuery, TResponse> : IRequestHandler<TQuery, PagedResult<TResponse>>
    where TEntity : class
    where TQuery : IRequest<PagedResult<TResponse>>, IPageableQuery
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListEntityHandlerBase{TEntity, TQuery, TResponse}"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    protected ListEntityHandlerBase(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <inheritdoc />
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

    /// <summary>
    /// Builds a QueryFilter from the query. Must be implemented by derived classes.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns>The QueryFilter.</returns>
    protected abstract QueryFilter<TEntity> BuildQueryFilter(TQuery query);

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
