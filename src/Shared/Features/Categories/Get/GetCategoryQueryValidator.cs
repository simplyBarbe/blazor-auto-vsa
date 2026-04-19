using FluentValidation;

namespace Shared.Features.Categories.Get;

public class GetCategoryQueryValidator : AbstractValidator<GetCategoryQuery>
{
    public GetCategoryQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("L'ID della categoria deve essere maggiore di zero");
    }
}
