using Shared.Core;
using Shared.Core.CRUD;

namespace Shared.Features.Products.Create;

public class CreateProductCommand : IRequest<Shared.Features.Products.Responses.ProductResponse>
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public CreateProductCommand() { }

    public CreateProductCommand(string name, decimal price)
    {
        Name = name;
        Price = price;
    }
}
