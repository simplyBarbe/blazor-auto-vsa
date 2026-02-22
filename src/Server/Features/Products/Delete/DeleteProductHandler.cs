using Server.Domain;
using Server.Infrastructure.CRUD.Handlers;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.Products.Delete;

namespace Server.Features.Products.Delete;

public class DeleteProductHandler : DeleteEntityHandlerBase<Product, DeleteProductCommand>
{
    public DeleteProductHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
    }
}
