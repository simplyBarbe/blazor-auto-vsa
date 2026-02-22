using Shared.Core;
using Shared.Core.CRUD;

namespace Shared.Features.Products.Update;

public class UpdateProductCommand : IRequest<Shared.Features.Products.Responses.ProductResponse>, IEntityKeyProvider
{
    public int Id { get; set; }
    public object[] GetKeys() => new object[] { Id };
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public UpdateProductCommand() { }

    public UpdateProductCommand(int id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }
}
