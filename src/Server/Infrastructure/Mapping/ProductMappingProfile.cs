using AutoMapper;
using Server.Domain;
using Shared.Features.Products.Create;
using Shared.Features.Products.Update;

namespace Server.Infrastructure.Mapping;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, Shared.Features.Products.Responses.ProductResponse>()
            .ForMember(d => d.GroupName, o => o.MapFrom(s => s.Group.Name))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Group.Category.Name))
            .ForMember(d => d.CategoryId, o => o.MapFrom(s => s.Group.CategoryId));
        CreateMap<CreateProductCommand, Product>();
        CreateMap<UpdateProductCommand, Product>();
    }
}
