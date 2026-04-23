using FluentValidation;
using Server.Domain;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.ProductGroups.Update;

namespace Server.Features.ProductGroups.Update;

public class UpdateProductGroupCommandServerValidator : AbstractValidator<UpdateProductGroupCommand>
{
    public UpdateProductGroupCommandServerValidator(IUnitOfWork unitOfWork)
    {
        Include(new UpdateProductGroupCommandValidator());

        RuleFor(x => x.CategoryId)
            .MustAsync(async (categoryId, cancellationToken) =>
            {
                var count = await unitOfWork.ReadRepository<Category>().CountAsync(
                    new QueryFilter<Category> { Filters = [c => c.Id == categoryId] },
                    cancellationToken);
                return count == 1;
            })
            .When(x => x.CategoryId > 0)
            .WithMessage("Selected category is not valid");

        RuleFor(x => x)
            .MustAsync(async (cmd, cancellationToken) =>
            {
                var count = await unitOfWork.ReadRepository<ProductGroup>().CountAsync(
                    new QueryFilter<ProductGroup>
                    {
                        Filters =
                        [
                            g => g.Id != cmd.Id
                                 && g.CategoryId == cmd.CategoryId
                                 && g.Name.ToLower() == cmd.Name.ToLower()
                        ]
                    },
                    cancellationToken);
                return count == 0;
            })
            .When(x => x.Id > 0 && x.CategoryId > 0 && !string.IsNullOrWhiteSpace(x.Name))
            .WithMessage("A group with this name already exists in the category");
    }
}
