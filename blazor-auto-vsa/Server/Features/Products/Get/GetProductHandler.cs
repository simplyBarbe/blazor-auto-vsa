using AutoMapper;
using Server.Domain;
using Server.Infrastructure.CRUD.Handlers;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.Products.Get;
using Shared.Features.Products.Responses;

namespace Server.Features.Products.Get;

/// <summary>
/// Handler for GetProductQuery - retrieves a product by ID.
/// </summary>
public class GetProductHandler : GetEntityHandlerBase<Product, GetProductQuery, ProductResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetProductHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public GetProductHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper)
    {
    }
}
