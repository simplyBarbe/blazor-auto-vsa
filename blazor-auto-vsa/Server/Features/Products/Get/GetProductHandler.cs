using AutoMapper;
using Server.Domain;
using Server.Infrastructure.CRUD.Handlers;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.Products.Get;
using Shared.Features.Products.Responses;

namespace Server.Features.Products.Get;

public class GetProductHandler : GetEntityHandlerBase<Product, GetProductQuery, ProductResponse>
{
    public GetProductHandler(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
    {
    }
}
