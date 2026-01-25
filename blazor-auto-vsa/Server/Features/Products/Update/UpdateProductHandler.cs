using AutoMapper;
using Server.Domain;
using Server.Infrastructure.CRUD.Handlers;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.Products.Update;
using Shared.Features.Products.Responses;

namespace Server.Features.Products.Update;

/// <summary>
/// Handler for UpdateProductCommand - updates an existing product.
/// </summary>
public class UpdateProductHandler : UpdateEntityHandlerBase<Product, UpdateProductCommand, ProductResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateProductHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public UpdateProductHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper)
    {
    }
}
