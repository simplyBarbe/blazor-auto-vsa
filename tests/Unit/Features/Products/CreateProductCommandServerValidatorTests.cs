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
            var command = new CreateProductCommand("Valid Product", 99.99m);
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
            await unitOfWork.WriteRepository<Product>().AddAsync(new Product { Name = "Existing Product", Price = 1m });
            await unitOfWork.SaveChangesAsync();

            var validator = new CreateProductCommandServerValidator(unitOfWork);
            var command = new CreateProductCommand("existing product", 99.99m);
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
            var validator = new CreateProductCommandServerValidator(unitOfWork);

            var command = new CreateProductCommand("", 10m);
            var result = await validator.ValidateAsync(command);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Il nome del prodotto è obbligatorio");
        }
    }
}
