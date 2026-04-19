using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Validation;
using Shared.Features.Products.Create;
using Shared.Features.Products.Get;
using Xunit;

namespace Unit.Features.Common;

public class FluentValidationRequestValidatorTests
{
    [Fact]
    public async Task ValidateAsync_when_no_validator_registered_should_return_success()
    {
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        var validator = new FluentValidationRequestValidator(provider);

        var request = new UnvalidatedRequest();
        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_when_validator_returns_valid_should_return_success()
    {
        var services = new ServiceCollection();
        services.AddScoped<FluentValidation.IValidator<GetProductQuery>, GetProductQueryValidator>();
        var provider = services.BuildServiceProvider();
        var validator = new FluentValidationRequestValidator(provider);

        var request = new GetProductQuery(1);
        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_when_validator_returns_invalid_should_return_failure_with_errors()
    {
        var services = new ServiceCollection();
        services.AddScoped<FluentValidation.IValidator<GetProductQuery>, GetProductQueryValidator>();
        var provider = services.BuildServiceProvider();
        var validator = new FluentValidationRequestValidator(provider);

        var request = new GetProductQuery(0);
        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Id" && e.ErrorMessage == "L'ID del prodotto deve essere maggiore di zero");
    }

    [Fact]
    public async Task ValidateAsync_with_create_command_validator_should_map_errors_correctly()
    {
        var services = new ServiceCollection();
        services.AddScoped<FluentValidation.IValidator<CreateProductCommand>, CreateProductCommandValidator>();
        var provider = services.BuildServiceProvider();
        var validator = new FluentValidationRequestValidator(provider);

        var request = new CreateProductCommand(1, "", -1m);
        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    private sealed class UnvalidatedRequest { }
}
