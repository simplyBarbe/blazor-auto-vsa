using AutoMapper;
using Server.Domain;
using Server.Infrastructure.Data.Contracts;
using Shared.Core;
using Shared.Core.CRUD;
using Shared.Core.Exceptions;
using Shared.Features.ProductGroups.Responses;
using Shared.Features.ProductGroups.Update;

namespace Server.Features.ProductGroups.Update;

public class UpdateProductGroupHandler : IRequestHandler<UpdateProductGroupCommand, ProductGroupResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly KeyExtractor _keyExtractor = KeyExtractor.Default;

    public UpdateProductGroupHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductGroupResponse> Handle(UpdateProductGroupCommand request, CancellationToken cancellationToken = default)
    {
        var keyValues = _keyExtractor.GetKeyValues(request);
        var entity = await _unitOfWork.ReadRepository<ProductGroup>().GetByKeyAsync(keyValues);

        if (entity is null)
        {
            throw new EntityNotFoundException(nameof(ProductGroup), keyValues);
        }

        _mapper.Map(request, entity);
        _unitOfWork.WriteRepository<ProductGroup>().Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ProjectAsync(entity.Id, cancellationToken);
    }

    private async Task<ProductGroupResponse> ProjectAsync(int id, CancellationToken cancellationToken)
    {
        var filter = new QueryFilter<ProductGroup>
        {
            Filters = [g => g.Id == id],
            Take = 1
        };

        var items = await _unitOfWork.ReadRepository<ProductGroup>().GetAsync<ProductGroupResponse>(filter, cancellationToken);
        if (items.Count == 0)
        {
            throw new EntityNotFoundException(nameof(ProductGroup), id);
        }

        return items[0];
    }
}
