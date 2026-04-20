using FluentValidation;
using Server.Domain;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.Categories.Delete;

namespace Server.Features.Categories.Delete;

public class DeleteCategoryCommandServerValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandServerValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Id)
            .MustAsync(async (id, cancellationToken) =>
            {
                var count = await unitOfWork.ReadRepository<ProductGroup>().CountAsync(
                    new QueryFilter<ProductGroup> { Filters = [g => g.CategoryId == id] },
                    cancellationToken);
                return count == 0;
            })
            .When(x => x.Id > 0)
            .WithMessage("Cannot delete category: linked groups exist");
    }
}
