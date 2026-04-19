using Shared.Core;
using Shared.Features.Categories.Responses;
namespace Shared.Features.Categories.Create;

public class CreateCategoryCommand : IRequest<CategoryResponse>
{
    public string Name { get; set; } = string.Empty;

    public CreateCategoryCommand() { }

    public CreateCategoryCommand(string name) => Name = name;
}
