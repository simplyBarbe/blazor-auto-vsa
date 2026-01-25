using Server.Domain;
using Server.Infrastructure.Data.Contracts;
using Shared.Core;
using Shared.Features.Products.Create;
using Shared.Features.Products.Responses;

namespace Server.Features.Products.Create;

/// <summary>
/// Handler for CreateProductCommand - creates a new product.
/// </summary>
public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateProductHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    public CreateProductHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Shared.Features.Products.Responses.ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price
        };

        await _unitOfWork.WriteRepository<Product>().AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new Shared.Features.Products.Responses.ProductResponse(product.Id, product.Name, product.Price);
    }
}
