using Server.Domain;
using Server.Infrastructure.CRUD.Handlers;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.Categories.Delete;

namespace Server.Features.Categories.Delete;

public class DeleteCategoryHandler : DeleteEntityHandlerBase<Category, DeleteCategoryCommand>
{
    public DeleteCategoryHandler(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}
