using FluentValidation;

namespace Shared.Features.Categories.Get;

public class GetCategoryQueryValidator : AbstractValidator<GetCategoryQuery>
{
    public GetCategoryQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Category ID must be greater than zero");
    }
}
