using AutoMapper;
using Server.Domain;
using Shared.Features.Products.Create;
using Shared.Features.Products.Update;

namespace Server.Infrastructure.Mapping;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, Shared.Features.Products.Responses.ProductResponse>();
        CreateMap<CreateProductCommand, Product>();
        CreateMap<UpdateProductCommand, Product>();
    }
}
