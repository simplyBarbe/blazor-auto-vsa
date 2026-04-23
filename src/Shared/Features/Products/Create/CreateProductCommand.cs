using Shared.Core;
using Shared.Core.CRUD;

namespace Shared.Features.Products.Create;

public class CreateProductCommand : IRequest<Shared.Features.Products.Responses.ProductResponse>
{
    public int GroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public CreateProductCommand() { }

    public CreateProductCommand(int groupId, string name, decimal price)
    {
        GroupId = groupId;
        Name = name;
        Price = price;
    }
}
