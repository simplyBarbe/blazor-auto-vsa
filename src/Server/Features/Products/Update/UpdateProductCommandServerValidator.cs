using FluentValidation;
using Server.Domain;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.Products.Update;

namespace Server.Features.Products.Update;

/// <summary>
/// Server-side validator for UpdateProductCommand.
/// Extends base validation with rules that require database access (e.g. unique name excluding the current product).
/// </summary>
public class UpdateProductCommandServerValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandServerValidator(IUnitOfWork unitOfWork)
    {
        Include(new UpdateProductCommandValidator());

        RuleFor(x => x.GroupId)
            .MustAsync(async (groupId, cancellationToken) =>
            {
                var count = await unitOfWork.ReadRepository<ProductGroup>().CountAsync(
                    new QueryFilter<ProductGroup> { Filters = [g => g.Id == groupId] },
                    cancellationToken);
                return count == 1;
            })
            .When(x => x.GroupId > 0)
            .WithMessage("Il gruppo selezionato non è valido");

        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) =>
            {
                var count = await unitOfWork.ReadRepository<Product>().CountAsync(
                    new QueryFilter<Product>
                    {
                        Filters =
                        [
                            p => p.Id != command.Id && p.Name.ToLower() == command.Name.ToLower()
                        ]
                    },
                    cancellationToken);
                return count == 0;
            })
            .When(x => !string.IsNullOrWhiteSpace(x.Name) && x.Id > 0)
            .WithMessage("Un prodotto con questo nome esiste già");
    }
}
