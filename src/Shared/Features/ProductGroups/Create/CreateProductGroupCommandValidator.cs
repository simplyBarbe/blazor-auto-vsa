using FluentValidation;

namespace Shared.Features.ProductGroups.Create;

public class CreateProductGroupCommandValidator : AbstractValidator<CreateProductGroupCommand>
{
    public CreateProductGroupCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("La categoria è obbligatoria");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Il nome del gruppo è obbligatorio")
            .MinimumLength(2).WithMessage("Il nome deve essere di almeno 2 caratteri")
            .MaximumLength(200).WithMessage("Il nome non può superare i 200 caratteri");
    }
}
