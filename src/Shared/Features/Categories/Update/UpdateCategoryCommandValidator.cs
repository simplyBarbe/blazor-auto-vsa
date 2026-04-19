using FluentValidation;

namespace Shared.Features.Categories.Update;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThanOrEqualTo(0).WithMessage("L'ID della categoria non può essere negativo");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Il nome della categoria è obbligatorio")
            .MinimumLength(2).WithMessage("Il nome deve essere di almeno 2 caratteri")
            .MaximumLength(200).WithMessage("Il nome non può superare i 200 caratteri");
    }
}
