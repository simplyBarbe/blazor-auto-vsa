using FluentValidation;
using Shared.Features.Products.Create;

namespace Server.Features.Products.Create;

/// <summary>
/// Server-side validator for CreateProductCommand.
/// Extends base validator with async rules that may require database access.
/// </summary>
public class CreateProductCommandServerValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandServerValidator()
    {
        // Include base validation rules from Shared
        Include(new CreateProductCommandValidator());

        // Add server-only async validation rules
        // Example: check if product name already exists in database
        RuleFor(x => x.Name)
            .MustAsync(async (name, cancellationToken) =>
            {
                // In real scenario: return !await _repository.ExistsByNameAsync(name, cancellationToken);
                await Task.Delay(1, cancellationToken);
                return true;
            })
            .WithMessage("Un prodotto con questo nome esiste già");
    }
}
