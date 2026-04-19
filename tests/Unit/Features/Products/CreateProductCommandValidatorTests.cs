using FluentAssertions;
using Shared.Features.Products.Create;
using Xunit;

namespace Unit.Features.Products;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void Valid_command_should_pass()
    {
        var command = new CreateProductCommand(1, "Valid Product", 99.99m);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Name_empty_or_whitespace_should_fail(string? name)
    {
        var command = new CreateProductCommand { GroupId = 1, Name = name ?? "", Price = 10m };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Il nome del prodotto è obbligatorio");
    }

    [Fact]
    public void Name_too_short_should_fail()
    {
        var command = new CreateProductCommand(1, "Ab", 10m);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Il nome deve essere di almeno 3 caratteri");
    }

    [Fact]
    public void Name_too_long_should_fail()
    {
        var command = new CreateProductCommand(1, new string('x', 101), 10m);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Il nome non può superare i 100 caratteri");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Price_zero_or_negative_should_fail(decimal price)
    {
        var command = new CreateProductCommand(1, "Product", price);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Il prezzo deve essere maggiore di zero");
    }

    [Fact]
    public void Price_too_many_digits_or_decimals_should_fail()
    {
        var command = new CreateProductCommand(1, "Product", 1234567.123m);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Il prezzo deve avere al massimo 8 cifre e 2 decimali");
    }

    [Fact]
    public void Price_over_max_should_fail()
    {
        var command = new CreateProductCommand(1, "Product", 1000000m);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Il prezzo non può superare 999999,99");
    }

    [Fact]
    public void Multiple_validation_errors_should_all_be_returned()
    {
        var command = new CreateProductCommand { GroupId = 0, Name = "", Price = -5m };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(1);
    }
}
