using FluentAssertions;
using Shared.Features.Products.Update;
using Xunit;

namespace Unit.Features.Products;

public class UpdateProductCommandValidatorTests
{
    private readonly UpdateProductCommandValidator _validator = new();

    [Fact]
    public void Valid_command_should_pass()
    {
        var command = new UpdateProductCommand(1, 1, "Valid Product", 99.99m);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Id_negative_should_fail()
    {
        var command = new UpdateProductCommand(-1, 1, "Product", 10m);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "L'ID del prodotto non può essere negativo");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Name_empty_or_whitespace_should_fail(string? name)
    {
        var command = new UpdateProductCommand(1, 1, name ?? "", 10m);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Il nome del prodotto è obbligatorio");
    }

    [Fact]
    public void Name_too_short_should_fail()
    {
        var command = new UpdateProductCommand(1, 1, "Ab", 10m);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Il nome deve essere di almeno 3 caratteri");
    }

    [Fact]
    public void Name_too_long_should_fail()
    {
        var command = new UpdateProductCommand(1, 1, new string('x', 101), 10m);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Il nome non può superare i 100 caratteri");
    }

    [Fact]
    public void Price_zero_should_fail()
    {
        var command = new UpdateProductCommand(1, 1, "Product", 0m);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Il prezzo deve essere maggiore di zero");
    }

    [Fact]
    public void Price_over_max_should_fail()
    {
        var command = new UpdateProductCommand(1, 1, "Product", 1000000m);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Il prezzo non può superare 999999,99");
    }
}
