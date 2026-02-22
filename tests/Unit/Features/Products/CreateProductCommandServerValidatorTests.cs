using FluentAssertions;
using Server.Features.Products.Create;
using Shared.Features.Products.Create;
using Xunit;

namespace Unit.Features.Products;

public class CreateProductCommandServerValidatorTests
{
    private readonly CreateProductCommandServerValidator _validator = new();

    [Fact]
    public async Task Valid_command_should_pass()
    {
        var command = new CreateProductCommand("Valid Product", 99.99m);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Empty_name_should_fail_from_included_shared_validator()
    {
        var command = new CreateProductCommand("", 10m);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Il nome del prodotto è obbligatorio");
    }
}
