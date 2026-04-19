using AutoMapper;
using Server.Domain;
using Server.Infrastructure.CRUD.Handlers;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.Categories.Responses;
using Shared.Features.Categories.Update;

namespace Server.Features.Categories.Update;

public class UpdateCategoryHandler : UpdateEntityHandlerBase<Category, UpdateCategoryCommand, CategoryResponse>
{
    public UpdateCategoryHandler(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
    {
    }
}
