using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Server.Infrastructure.Data;
using Server.Infrastructure.Data.Contracts;
using Server.Infrastructure.Data.Repositories;

namespace Unit.Features.Products;

internal static class ProductValidatorTestFactory
{
    public static (ApplicationDbContext Context, IUnitOfWork UnitOfWork) CreateUnitOfWork()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        // ReadRepository.CountAsync does not use IMapper; a stub is sufficient for UnitOfWork construction.
        var mapper = Mock.Of<IMapper>();
        var unitOfWork = new UnitOfWork(context, mapper);
        return (context, unitOfWork);
    }
}
