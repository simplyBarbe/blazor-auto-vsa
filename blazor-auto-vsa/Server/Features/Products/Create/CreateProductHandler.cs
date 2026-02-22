using AutoMapper;
using Server.Domain;
using Server.Infrastructure.CRUD.Handlers;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.Products.Create;
using Shared.Features.Products.Responses;

namespace Server.Features.Products.Create;

public class CreateProductHandler : CreateEntityHandlerBase<Product, CreateProductCommand, ProductResponse>
{
    public CreateProductHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper)
    {
    }
}
