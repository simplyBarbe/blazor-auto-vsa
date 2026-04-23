using Server.Domain;
using Server.Infrastructure.Data.Contracts;
using Shared.Core;
using Shared.Core.CRUD;
using Shared.Core.Exceptions;
using Shared.Features.ProductGroups.Get;
using Shared.Features.ProductGroups.Responses;

namespace Server.Features.ProductGroups.Get;

public class GetProductGroupHandler : IRequestHandler<GetProductGroupQuery, ProductGroupResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductGroupHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductGroupResponse> Handle(GetProductGroupQuery request, CancellationToken cancellationToken = default)
    {
        var filter = new QueryFilter<ProductGroup>
        {
            Filters = [g => g.Id == request.Id],
            Take = 1
        };

        var items = await _unitOfWork.ReadRepository<ProductGroup>().GetAsync<ProductGroupResponse>(filter, cancellationToken);
        if (items.Count == 0)
        {
            throw new EntityNotFoundException(nameof(ProductGroup), request.Id);
        }

        return items[0];
    }
}
