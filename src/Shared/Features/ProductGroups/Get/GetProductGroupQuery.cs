using Shared.Core;
using Shared.Core.CRUD;
using Shared.Features.ProductGroups.Responses;

namespace Shared.Features.ProductGroups.Get;

public class GetProductGroupQuery : IRequest<ProductGroupResponse>, IEntityKeyProvider
{
    public int Id { get; set; }
    public object[] GetKeys() => new object[] { Id };

    public GetProductGroupQuery(int id) => Id = id;
}
