using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Domain;
using Server.Infrastructure.Data;
using Server.Infrastructure.Data.Contracts;
using Shared.Core;
using Shared.Core.Exceptions;
using Shared.Features.Products.Create;
using Shared.Features.Products.Responses;

namespace Server.Features.Products.Create;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;

    public CreateProductHandler(IUnitOfWork unitOfWork, IMapper mapper, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
    }

    public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Product>(request);
        await _unitOfWork.WriteRepository<Product>().AddAsync(entity, cancellationToken);
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
