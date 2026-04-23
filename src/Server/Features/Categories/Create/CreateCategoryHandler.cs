using AutoMapper;
using Server.Domain;
using Server.Infrastructure.CRUD.Handlers;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.Categories.Create;
using Shared.Features.Categories.Responses;

namespace Server.Features.Categories.Create;

public class CreateCategoryHandler : CreateEntityHandlerBase<Category, CreateCategoryCommand, CategoryResponse>
{
    public CreateCategoryHandler(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
    {
    }
}
