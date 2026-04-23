using Microsoft.EntityFrameworkCore;
using Server.Domain;
using Server.Infrastructure.Data;
using Shared.Core;
using Shared.Core.Exceptions;
using Shared.Features.Products.Get;
using Shared.Features.Products.Responses;

namespace Server.Features.Products.Get;

public class GetProductHandler : IRequestHandler<GetProductQuery, ProductResponse>
{
    private readonly ApplicationDbContext _context;

    public GetProductHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductResponse> Handle(GetProductQuery request, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.AsNoTracking()
            .Where(p => p.Id == request.Id)
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
            throw new EntityNotFoundException(nameof(Product), request.Id);
        }

        return product;
    }
}
