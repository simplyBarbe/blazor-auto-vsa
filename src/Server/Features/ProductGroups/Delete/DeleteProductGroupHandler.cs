using Server.Domain;
using Server.Infrastructure.CRUD.Handlers;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.ProductGroups.Delete;

namespace Server.Features.ProductGroups.Delete;

public class DeleteProductGroupHandler : DeleteEntityHandlerBase<ProductGroup, DeleteProductGroupCommand>
{
    public DeleteProductGroupHandler(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}
