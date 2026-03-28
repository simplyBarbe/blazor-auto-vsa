using FluentValidation;
using Server.Domain;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.Products.Create;

namespace Server.Features.Products.Create;

/// <summary>
/// Server-side validator for CreateProductCommand.
/// Extends base validation with rules that require database access (e.g. unique name).
/// </summary>
public class CreateProductCommandServerValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandServerValidator(IUnitOfWork unitOfWork)
    {
        Include(new CreateProductCommandValidator());

        RuleFor(x => x.Name)
            .MustAsync(async (name, cancellationToken) =>
            {
                var count = await unitOfWork.ReadRepository<Product>().CountAsync(
                    new QueryFilter<Product>
                    {
                        Filters = [p => p.Name.ToLower() == name.ToLower()]
                    },
                    cancellationToken);
                return count == 0;
            })
            .When(x => !string.IsNullOrWhiteSpace(x.Name))
            .WithMessage("Un prodotto con questo nome esiste già");
    }
}
