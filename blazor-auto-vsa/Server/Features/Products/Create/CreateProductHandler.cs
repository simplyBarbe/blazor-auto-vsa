using AutoMapper;
using Server.Domain;
using Server.Infrastructure.CRUD.Handlers;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.Products.Create;
using Shared.Features.Products.Responses;

namespace Server.Features.Products.Create;

/// <summary>
/// Handler for CreateProductCommand - creates a new product.
/// </summary>
public class CreateProductHandler : CreateEntityHandlerBase<Product, CreateProductCommand, ProductResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateProductHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public CreateProductHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper)
    {
    }
}
