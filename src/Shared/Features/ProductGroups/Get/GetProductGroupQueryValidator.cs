using FluentValidation;

namespace Shared.Features.ProductGroups.Get;

public class GetProductGroupQueryValidator : AbstractValidator<GetProductGroupQuery>
{
    public GetProductGroupQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("L'ID del gruppo deve essere maggiore di zero");
    }
}
