using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Domain;
using Server.Infrastructure.Data;
using Server.Infrastructure.Data.Contracts;
using Shared.Core;
using Shared.Core.CRUD;
using Shared.Core.Exceptions;
using Shared.Features.Products.Responses;
using Shared.Features.Products.Update;

namespace Server.Features.Products.Update;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, ProductResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;
    private readonly KeyExtractor _keyExtractor = KeyExtractor.Default;

    public UpdateProductHandler(IUnitOfWork unitOfWork, IMapper mapper, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
    }

    public async Task<ProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken = default)
    {
        var keyValues = _keyExtractor.GetKeyValues(request);
        var entity = await _unitOfWork.ReadRepository<Product>().GetByKeyAsync(keyValues);

        if (entity is null)
        {
            throw new EntityNotFoundException(nameof(Product), keyValues);
        }

        _mapper.Map(request, entity);
        _unitOfWork.WriteRepository<Product>().Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ProjectProductAsync(entity.Id, cancellationToken);
    }

    private async Task<ProductResponse> ProjectProductAsync(int id, CancellationToken cancellationToken)
    {
        var product = await _context.Products.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Price,
                p.GroupId,
                p.Group.CategoryId,
                p.Group.Category.Name,
                p.Group.Name))
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            throw new EntityNotFoundException(nameof(Product), id);
        }

        return product;
    }
}
