using Shared.Core;
using Shared.Core.CRUD;

namespace Shared.Features.Products.Get;

public class GetProductQuery : IRequest<Shared.Features.Products.Responses.ProductResponse>, IEntityKeyProvider
{
    public int Id { get; set; }
    public object[] GetKeys() => new object[] { Id };

    public GetProductQuery() { }

    public GetProductQuery(int id)
    {
        Id = id;
    }
}
