using FluentAssertions;
using Shared.Features.Products.Get;
using Xunit;

namespace Unit.Features.Products;

public class GetProductQueryValidatorTests
{
    private readonly GetProductQueryValidator _validator = new();

    [Fact]
    public void Valid_id_should_pass()
    {
        var query = new GetProductQuery(1);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Id_zero_should_fail()
    {
        var query = new GetProductQuery(0);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Product ID must be greater than zero");
    }

    [Fact]
    public void Id_negative_should_fail()
    {
        var query = new GetProductQuery(-1);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Product ID must be greater than zero");
    }
}
