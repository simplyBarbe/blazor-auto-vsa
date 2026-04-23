using FluentValidation;
using Server.Domain;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.Categories.Create;

namespace Server.Features.Categories.Create;

public class CreateCategoryCommandServerValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandServerValidator(IUnitOfWork unitOfWork)
    {
        Include(new CreateCategoryCommandValidator());

        RuleFor(x => x.Name)
            .MustAsync(async (name, cancellationToken) =>
            {
                var count = await unitOfWork.ReadRepository<Category>().CountAsync(
                    new QueryFilter<Category> { Filters = [c => c.Name.ToLower() == name.ToLower()] },
                    cancellationToken);
                return count == 0;
            })
            .When(x => !string.IsNullOrWhiteSpace(x.Name))
            .WithMessage("A category with this name already exists");
    }
}
