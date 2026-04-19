using AutoMapper;
using Server.Domain;
using Server.Infrastructure.Data.Contracts;
using Shared.Core;
using Shared.Core.CRUD;
using Shared.Core.Exceptions;
using Shared.Features.ProductGroups.Create;
using Shared.Features.ProductGroups.Responses;

namespace Server.Features.ProductGroups.Create;

public class CreateProductGroupHandler : IRequestHandler<CreateProductGroupCommand, ProductGroupResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateProductGroupHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductGroupResponse> Handle(CreateProductGroupCommand request, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<ProductGroup>(request);
        await _unitOfWork.WriteRepository<ProductGroup>().AddAsync(entity, cancellationToken);
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
