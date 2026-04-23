using FluentValidation;
using Server.Domain;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.ProductGroups.Delete;

namespace Server.Features.ProductGroups.Delete;

public class DeleteProductGroupCommandServerValidator : AbstractValidator<DeleteProductGroupCommand>
{
    public DeleteProductGroupCommandServerValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Id)
            .MustAsync(async (id, cancellationToken) =>
            {
                var count = await unitOfWork.ReadRepository<Product>().CountAsync(
                    new QueryFilter<Product> { Filters = [p => p.GroupId == id] },
                    cancellationToken);
                return count == 0;
            })
            .When(x => x.Id > 0)
            .WithMessage("Cannot delete group: linked products exist");
    }
}
