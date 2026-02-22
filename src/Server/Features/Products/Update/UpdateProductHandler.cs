using AutoMapper;
using Server.Domain;
using Server.Infrastructure.CRUD.Handlers;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.Products.Update;
using Shared.Features.Products.Responses;

namespace Server.Features.Products.Update;

public class UpdateProductHandler : UpdateEntityHandlerBase<Product, UpdateProductCommand, ProductResponse>
{
    public UpdateProductHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper)
    {
    }
}
