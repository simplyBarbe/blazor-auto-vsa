using FluentValidation;

namespace Shared.Features.Products.Create;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.GroupId)
            .GreaterThan(0).WithMessage("Group is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Price)
            .PrecisionScale(8, 2, false).WithMessage("Price must have at most 8 digits and 2 decimals")
            .GreaterThan(0).WithMessage("Price must be greater than zero")
            .LessThanOrEqualTo(999999.99m).WithMessage("Price cannot exceed 999999.99");
    }
}
