using AutoMapper;
using Server.Domain;

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
    }
}
