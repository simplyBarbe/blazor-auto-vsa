using FluentValidation;

namespace Shared.Features.Products.Create;

/// <summary>
/// Base validator for CreateProductCommand with simple synchronous rules.
/// Used by both client and server.
/// </summary>
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Il nome del prodotto è obbligatorio")
            .MinimumLength(3).WithMessage("Il nome deve essere di almeno 3 caratteri")
            .MaximumLength(100).WithMessage("Il nome non può superare i 100 caratteri");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Il prezzo deve essere maggiore di zero")
            .LessThanOrEqualTo(999999.99m).WithMessage("Il prezzo non può superare 999999,99");
    }
}
