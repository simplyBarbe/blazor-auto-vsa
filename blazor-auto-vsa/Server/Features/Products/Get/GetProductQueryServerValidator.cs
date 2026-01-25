using FluentValidation;
using Shared.Features.Products.Get;

namespace Server.Features.Products.Get;

/// <summary>
/// Server-side validator for GetProductQuery.
/// Extends base validator with async rules that may require database access.
/// </summary>
public class GetProductQueryServerValidator : AbstractValidator<GetProductQuery>
{
    public GetProductQueryServerValidator()
    {
        // Include base validation rules from Shared
        Include(new GetProductQueryValidator());

        // Add server-only async validation rules
        // Example: check if product exists in database
        RuleFor(x => x.Id)
            .MustAsync(async (id, cancellationToken) =>
            {
                // In real scenario: return await _repository.ExistsAsync(id, cancellationToken);
                await Task.Delay(1, cancellationToken);
                return true;
            })
            .WithMessage("Il prodotto specificato non esiste");
    }
}
