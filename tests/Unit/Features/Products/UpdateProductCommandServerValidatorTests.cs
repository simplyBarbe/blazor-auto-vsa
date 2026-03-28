using FluentAssertions;
using Server.Domain;
using Server.Features.Products.Update;
using Shared.Features.Products.Update;
using Xunit;

namespace Unit.Features.Products;

public class UpdateProductCommandServerValidatorTests
{
    [Fact]
    public async Task Valid_command_should_pass_when_name_is_unique_for_other_products()
    {
        var (context, unitOfWork) = ProductValidatorTestFactory.CreateUnitOfWork();
        await using (context)
        {
            await unitOfWork.WriteRepository<Product>().AddAsync(new Product { Name = "Other", Price = 1m });
            await unitOfWork.SaveChangesAsync();

            var validator = new UpdateProductCommandServerValidator(unitOfWork);
            var command = new UpdateProductCommand(1, "Unique Name", 99.99m);
            var result = await validator.ValidateAsync(command);
            result.IsValid.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Same_id_same_name_should_pass()
    {
        var (context, unitOfWork) = ProductValidatorTestFactory.CreateUnitOfWork();
        await using (context)
        {
            var product = new Product { Name = "My Product", Price = 5m };
            await unitOfWork.WriteRepository<Product>().AddAsync(product);
            await unitOfWork.SaveChangesAsync();

            var validator = new UpdateProductCommandServerValidator(unitOfWork);
            var command = new UpdateProductCommand(product.Id, "my product", 10m);
            var result = await validator.ValidateAsync(command);
            result.IsValid.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Duplicate_name_on_different_id_should_fail_case_insensitively()
    {
        var (context, unitOfWork) = ProductValidatorTestFactory.CreateUnitOfWork();
        await using (context)
        {
            await unitOfWork.WriteRepository<Product>().AddAsync(new Product { Name = "First", Price = 1m });
            var second = new Product { Name = "Second", Price = 2m };
            await unitOfWork.WriteRepository<Product>().AddAsync(second);
            await unitOfWork.SaveChangesAsync();

            var validator = new UpdateProductCommandServerValidator(unitOfWork);
            var command = new UpdateProductCommand(second.Id, "FIRST", 99m);
            var result = await validator.ValidateAsync(command);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Un prodotto con questo nome esiste già");
        }
    }

    [Fact]
    public async Task Empty_name_should_fail_from_included_shared_validator()
    {
        var (context, unitOfWork) = ProductValidatorTestFactory.CreateUnitOfWork();
        await using (context)
        {
            var validator = new UpdateProductCommandServerValidator(unitOfWork);
            var command = new UpdateProductCommand(1, "", 10m);
            var result = await validator.ValidateAsync(command);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Il nome del prodotto è obbligatorio");
        }
    }
}
