using FluentValidation;
using Server.Domain;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.Categories.Update;

namespace Server.Features.Categories.Update;

public class UpdateCategoryCommandServerValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandServerValidator(IUnitOfWork unitOfWork)
    {
        Include(new UpdateCategoryCommandValidator());

        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("L'ID della categoria deve essere maggiore di zero");

        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) =>
            {
                var count = await unitOfWork.ReadRepository<Category>().CountAsync(
                    new QueryFilter<Category>
                    {
                        Filters =
                        [
                            c => c.Id != command.Id
                                 && c.Name.ToLower() == command.Name.ToLower()
                        ]
                    },
                    cancellationToken);
                return count == 0;
            })
            .When(x => !string.IsNullOrWhiteSpace(x.Name) && x.Id > 0)
            .WithMessage("Esiste già una categoria con questo nome");
    }
}
