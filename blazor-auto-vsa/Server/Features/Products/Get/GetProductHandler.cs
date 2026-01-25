using Server.Domain;
using Server.Infrastructure.Data.Contracts;
using Shared.Core;
using Shared.Core.Exceptions;
using Shared.Features.Products.Get;
using Shared.Features.Products.Responses;

namespace Server.Features.Products.Get;

/// <summary>
/// Handler for GetProductQuery - retrieves a product by ID.
/// </summary>
public class GetProductHandler : IRequestHandler<GetProductQuery, ProductResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetProductHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    public GetProductHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Shared.Features.Products.Responses.ProductResponse> Handle(GetProductQuery request, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.ReadRepository<Product>().GetByKeyAsync(request.Id);

        if (product is null)
        {
            throw new EntityNotFoundException(nameof(Product), request.Id);
        }

        return new Shared.Features.Products.Responses.ProductResponse(product.Id, product.Name, product.Price);
    }
}
