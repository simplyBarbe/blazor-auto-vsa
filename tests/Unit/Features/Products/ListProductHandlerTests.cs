using AutoMapper;
using FluentAssertions;
using Moq;
using Server.Domain;
using Server.Features.Products.List;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.Products.List;
using Shared.Features.Products.Responses;
using Xunit;

namespace Unit.Features.Products;

public class ListProductHandlerTests
{
    private static IMapper CreateMapper() => new Mock<IMapper>().Object;

    [Fact]
    public async Task Handle_should_use_default_paging_when_page_not_specified()
    {
        QueryFilter<Product>? capturedFilter = null;
        var mockReadRepo = new Mock<IReadRepository<Product>>();
        mockReadRepo
            .Setup(x => x.GetAsync<ProductResponse>(It.IsAny<QueryFilter<Product>>(), It.IsAny<CancellationToken>()))
            .Callback<QueryFilter<Product>, CancellationToken>((f, _) => capturedFilter = f)
            .ReturnsAsync(new List<ProductResponse>());
        mockReadRepo
            .Setup(x => x.CountAsync(It.IsAny<QueryFilter<Product>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(x => x.ReadRepository<Product>()).Returns(mockReadRepo.Object);

        var handler = new ListProductHandler(mockUow.Object, CreateMapper());
        var query = new ListProductQuery(); // PageNumber=1, PageSize=10 by default

        await handler.Handle(query);

        capturedFilter.Should().NotBeNull();
        capturedFilter!.Skip.Should().Be(0);
        capturedFilter.Take.Should().Be(10);
        capturedFilter.OrderBy.Should().NotBeNull().And.HaveCount(1);
    }

    [Fact]
    public async Task Handle_should_apply_custom_paging()
    {
        QueryFilter<Product>? capturedFilter = null;
        var mockReadRepo = new Mock<IReadRepository<Product>>();
        mockReadRepo
            .Setup(x => x.GetAsync<ProductResponse>(It.IsAny<QueryFilter<Product>>(), It.IsAny<CancellationToken>()))
            .Callback<QueryFilter<Product>, CancellationToken>((f, _) => capturedFilter = f)
            .ReturnsAsync(new List<ProductResponse>());
        mockReadRepo
            .Setup(x => x.CountAsync(It.IsAny<QueryFilter<Product>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(25);

        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(x => x.ReadRepository<Product>()).Returns(mockReadRepo.Object);

        var handler = new ListProductHandler(mockUow.Object, CreateMapper());
        var query = new ListProductQuery { PageNumber = 3, PageSize = 5 };

        var result = await handler.Handle(query);

        capturedFilter.Should().NotBeNull();
        capturedFilter!.Skip.Should().Be((3 - 1) * 5); // 10
        capturedFilter.Take.Should().Be(5);
        result.PageNumber.Should().Be(3);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(25);
    }

    [Fact]
    public async Task Handle_should_apply_search_term_filter()
    {
        QueryFilter<Product>? capturedFilter = null;
        var mockReadRepo = new Mock<IReadRepository<Product>>();
        mockReadRepo
            .Setup(x => x.GetAsync<ProductResponse>(It.IsAny<QueryFilter<Product>>(), It.IsAny<CancellationToken>()))
            .Callback<QueryFilter<Product>, CancellationToken>((f, _) => capturedFilter = f)
            .ReturnsAsync(new List<ProductResponse>());
        mockReadRepo
            .Setup(x => x.CountAsync(It.IsAny<QueryFilter<Product>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(x => x.ReadRepository<Product>()).Returns(mockReadRepo.Object);

        var handler = new ListProductHandler(mockUow.Object, CreateMapper());
        var query = new ListProductQuery { SearchTerm = "widget" };

        await handler.Handle(query);

        capturedFilter.Should().NotBeNull();
        capturedFilter!.Filters.Should().NotBeNull().And.HaveCount(1);
    }

    [Fact]
    public async Task Handle_should_return_paged_result_with_items()
    {
        var items = new List<ProductResponse> { new(1, "A", 10m), new(2, "B", 20m) };
        var mockReadRepo = new Mock<IReadRepository<Product>>();
        mockReadRepo
            .Setup(x => x.GetAsync<ProductResponse>(It.IsAny<QueryFilter<Product>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);
        mockReadRepo
            .Setup(x => x.CountAsync(It.IsAny<QueryFilter<Product>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(x => x.ReadRepository<Product>()).Returns(mockReadRepo.Object);

        var handler = new ListProductHandler(mockUow.Object, CreateMapper());
        var result = await handler.Handle(new ListProductQuery());

        result.Items.Should().BeEquivalentTo(items);
        result.TotalCount.Should().Be(2);
        result.TotalPages.Should().Be(1);
    }
}
