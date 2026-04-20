using FluentAssertions;
using Server.Domain;
using Server.Features.Products.Create;
using Shared.Features.Products.Create;
using Xunit;

namespace Unit.Features.Products;

public class CreateProductCommandServerValidatorTests
{
    [Fact]
    public async Task Valid_command_should_pass_when_name_is_unique()
    {
        var (context, unitOfWork) = ProductValidatorTestFactory.CreateUnitOfWork();
        await using (context)
        {
            var validator = new CreateProductCommandServerValidator(unitOfWork);
            var command = new CreateProductCommand(ProductValidatorTestFactory.DefaultGroupId, "Valid Product", 99.99m);
            var result = await validator.ValidateAsync(command);
            result.IsValid.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Duplicate_name_should_fail_case_insensitively()
    {
        var (context, unitOfWork) = ProductValidatorTestFactory.CreateUnitOfWork();
        await using (context)
        {
            await unitOfWork.WriteRepository<Product>().AddAsync(new Product
            {
                GroupId = ProductValidatorTestFactory.DefaultGroupId,
                Name = "Existing Product",
                Price = 1m
            });
            await unitOfWork.SaveChangesAsync();

            var validator = new CreateProductCommandServerValidator(unitOfWork);
            var command = new CreateProductCommand(ProductValidatorTestFactory.DefaultGroupId, "existing product", 99.99m);
            var result = await validator.ValidateAsync(command);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "A product with this name already exists");
        }
    }

    [Fact]
    public async Task Empty_name_should_fail_from_included_shared_validator()
    {
        var (context, unitOfWork) = ProductValidatorTestFactory.CreateUnitOfWork();
        await using (context)
        {
            var validator = new CreateProductCommandServerValidator(unitOfWork);

            var command = new CreateProductCommand(ProductValidatorTestFactory.DefaultGroupId, "", 10m);
            var result = await validator.ValidateAsync(command);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Product name is required");
        }
    }
}
