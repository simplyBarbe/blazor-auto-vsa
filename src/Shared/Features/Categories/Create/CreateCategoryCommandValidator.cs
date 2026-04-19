using FluentValidation;

namespace Shared.Features.Categories.Create;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Il nome della categoria è obbligatorio")
            .MinimumLength(2).WithMessage("Il nome deve essere di almeno 2 caratteri")
            .MaximumLength(200).WithMessage("Il nome non può superare i 200 caratteri");
    }
}
