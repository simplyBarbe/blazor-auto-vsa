using FluentValidation;
using Server.Domain;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.ProductGroups.Create;

namespace Server.Features.ProductGroups.Create;

public class CreateProductGroupCommandServerValidator : AbstractValidator<CreateProductGroupCommand>
{
    public CreateProductGroupCommandServerValidator(IUnitOfWork unitOfWork)
    {
        Include(new CreateProductGroupCommandValidator());

        RuleFor(x => x.CategoryId)
            .MustAsync(async (categoryId, cancellationToken) =>
            {
                var count = await unitOfWork.ReadRepository<Category>().CountAsync(
                    new QueryFilter<Category> { Filters = [c => c.Id == categoryId] },
                    cancellationToken);
                return count == 1;
            })
            .When(x => x.CategoryId > 0)
            .WithMessage("La categoria selezionata non è valida");

        RuleFor(x => x)
            .MustAsync(async (cmd, cancellationToken) =>
            {
                var count = await unitOfWork.ReadRepository<ProductGroup>().CountAsync(
                    new QueryFilter<ProductGroup>
                    {
                        Filters =
                        [
                            g => g.CategoryId == cmd.CategoryId
                                 && g.Name.ToLower() == cmd.Name.ToLower()
                        ]
                    },
                    cancellationToken);
                return count == 0;
            })
            .When(x => x.CategoryId > 0 && !string.IsNullOrWhiteSpace(x.Name))
            .WithMessage("Esiste già un gruppo con questo nome nella categoria");
    }
}
