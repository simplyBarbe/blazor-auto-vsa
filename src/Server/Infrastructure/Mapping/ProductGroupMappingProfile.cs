using AutoMapper;
using Server.Domain;
using Shared.Features.ProductGroups.Create;
using Shared.Features.ProductGroups.Responses;
using Shared.Features.ProductGroups.Update;

namespace Server.Infrastructure.Mapping;

public class ProductGroupMappingProfile : Profile
{
    public ProductGroupMappingProfile()
    {
        CreateMap<ProductGroup, ProductGroupResponse>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name));
        CreateMap<CreateProductGroupCommand, ProductGroup>();
        CreateMap<UpdateProductGroupCommand, ProductGroup>();
    }
}
