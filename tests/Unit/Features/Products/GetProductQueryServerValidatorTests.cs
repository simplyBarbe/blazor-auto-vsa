using FluentAssertions;
using Server.Features.Products.Get;
using Shared.Features.Products.Get;
using Xunit;

namespace Unit.Features.Products;

public class GetProductQueryServerValidatorTests
{
    private readonly GetProductQueryServerValidator _validator = new();

    [Fact]
    public async Task Valid_id_should_pass()
    {
        var query = new GetProductQuery(1);
        var result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Id_zero_should_fail_from_included_shared_validator()
    {
        var query = new GetProductQuery(0);
        var result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "L'ID del prodotto deve essere maggiore di zero");
    }
}
