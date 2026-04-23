using FluentValidation;

namespace Shared.Features.Products.Get;

/// <summary>
/// Base validator for GetProductQuery with simple synchronous rules.
/// Used by both client and server.
/// </summary>
public class GetProductQueryValidator : AbstractValidator<GetProductQuery>
{
    public GetProductQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Product ID must be greater than zero");
    }
}
