using AutoMapper;
using Server.Domain;
using Shared.Features.Products.Create;
using Shared.Features.Products.Update;

namespace Server.Infrastructure.Mapping;

/// <summary>
/// AutoMapper profile for Product entity mappings.
/// </summary>
public class ProductMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductMappingProfile"/> class.
    /// </summary>
    public ProductMappingProfile()
    {
        CreateMap<Product, Shared.Features.Products.Responses.ProductResponse>();
        CreateMap<CreateProductCommand, Product>();
        CreateMap<UpdateProductCommand, Product>();
    }
}
