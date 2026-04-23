using FluentValidation;

namespace Shared.Features.Categories.Update;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThanOrEqualTo(0).WithMessage("Category ID cannot be negative");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");
    }
}
