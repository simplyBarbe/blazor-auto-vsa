using AutoMapper;
using Server.Domain;
using Shared.Features.Categories.Create;
using Shared.Features.Categories.Responses;
using Shared.Features.Categories.Update;

namespace Server.Infrastructure.Mapping;

public class CategoryMappingProfile : Profile
{
    public CategoryMappingProfile()
    {
        CreateMap<Category, CategoryResponse>();
        CreateMap<CreateCategoryCommand, Category>();
        CreateMap<UpdateCategoryCommand, Category>();
    }
}
