using FluentValidation;

namespace Shared.Features.ProductGroups.Update;

public class UpdateProductGroupCommandValidator : AbstractValidator<UpdateProductGroupCommand>
{
    public UpdateProductGroupCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("L'ID del gruppo deve essere maggiore di zero");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("La categoria è obbligatoria");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Il nome del gruppo è obbligatorio")
            .MinimumLength(2).WithMessage("Il nome deve essere di almeno 2 caratteri")
            .MaximumLength(200).WithMessage("Il nome non può superare i 200 caratteri");
    }
}
