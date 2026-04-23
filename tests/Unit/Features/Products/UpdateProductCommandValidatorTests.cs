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
        result.Errors.Should().Contain(e => e.ErrorMessage == "Product ID cannot be negative");
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
        result.Errors.Should().Contain(e => e.ErrorMessage == "Product name is required");
    }

    [Fact]
    public void Name_too_short_should_fail()
    {
        var command = new UpdateProductCommand(1, 1, "Ab", 10m);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Name must be at least 3 characters");
    }

    [Fact]
    public void Name_too_long_should_fail()
    {
        var command = new UpdateProductCommand(1, 1, new string('x', 101), 10m);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Name cannot exceed 100 characters");
    }

    [Fact]
    public void Price_zero_should_fail()
    {
        var command = new UpdateProductCommand(1, 1, "Product", 0m);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Price must be greater than zero");
    }

    [Fact]
    public void Price_over_max_should_fail()
    {
        var command = new UpdateProductCommand(1, 1, "Product", 1000000m);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Price cannot exceed 999999.99");
    }
}
