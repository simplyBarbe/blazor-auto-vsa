using FluentValidation;

namespace Shared.Features.Products.Create;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Il nome del prodotto è obbligatorio")
            .MinimumLength(3).WithMessage("Il nome deve essere di almeno 3 caratteri")
            .MaximumLength(100).WithMessage("Il nome non può superare i 100 caratteri");

        RuleFor(x => x.Price)
            .PrecisionScale(8, 2, false).WithMessage("Il prezzo deve avere al massimo 8 cifre e 2 decimali")
            .GreaterThan(0).WithMessage("Il prezzo deve essere maggiore di zero")
            .LessThanOrEqualTo(999999.99m).WithMessage("Il prezzo non può superare 999999,99");
    }
}
