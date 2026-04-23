using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Server.Domain;
using Server.Infrastructure.Data;
using Server.Infrastructure.Data.Contracts;
using Server.Infrastructure.Data.Repositories;

namespace Unit.Features.Products;

internal static class ProductValidatorTestFactory
{
    /// <summary>Default group id after <see cref="SeedMinimalTaxonomy"/> (first ProductGroup row).</summary>
    public const int DefaultGroupId = 1;

    public static (ApplicationDbContext Context, IUnitOfWork UnitOfWork) CreateUnitOfWork()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        SeedMinimalTaxonomy(context);

        // ReadRepository.CountAsync does not use IMapper; a stub is sufficient for UnitOfWork construction.
        var mapper = Mock.Of<IMapper>();
        var unitOfWork = new UnitOfWork(context, mapper);
        return (context, unitOfWork);
    }

    private static void SeedMinimalTaxonomy(ApplicationDbContext context)
    {
        var cat = new Category { Name = "UnitTestCategory" };
        context.Categories.Add(cat);
        context.SaveChanges();
        context.ProductGroups.Add(new ProductGroup { CategoryId = cat.Id, Name = "UnitTestGroup" });
        context.SaveChanges();
    }
}
