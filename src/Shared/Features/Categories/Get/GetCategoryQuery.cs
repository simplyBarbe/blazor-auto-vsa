using Shared.Core;
using Shared.Core.CRUD;
using Shared.Features.Categories.Responses;

namespace Shared.Features.Categories.Get;

public class GetCategoryQuery : IRequest<CategoryResponse>, IEntityKeyProvider
{
    public int Id { get; set; }
    public object[] GetKeys() => new object[] { Id };

    public GetCategoryQuery(int id) => Id = id;
}
