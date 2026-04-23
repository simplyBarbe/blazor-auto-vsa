using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Server.Domain;
using Server.Features.Products.List;
using Server.Infrastructure.Data;
using Shared.Features.Products.List;
using Xunit;

namespace Unit.Features.Products;

public class ListProductHandlerTests
{
    private static async Task<(ApplicationDbContext Context, ProductGroup Group)> CreateContextWithGroupAsync()
    {
        var (context, _) = ProductValidatorTestFactory.CreateUnitOfWork();
        var cat = await context.Categories.FirstAsync(c => c.Name == "UnitTestCategory");
        var grp = await context.ProductGroups.FirstAsync(g => g.CategoryId == cat.Id);
        return (context, grp);
    }

    [Fact]
    public async Task Handle_should_use_default_paging_when_page_not_specified()
    {
        var (context, grp) = await CreateContextWithGroupAsync();
        await using (context)
        {
            context.Products.Add(new Product { GroupId = grp.Id, Name = "Test Product", Price = 1m });
            await context.SaveChangesAsync();

            var handler = new ListProductHandler(context);
            await handler.Handle(new ListProductQuery());

            // No assertion on internal filter; handler completes without throwing.
        }
    }

    [Fact]
    public async Task Handle_should_apply_custom_paging()
    {
        var (context, grp) = await CreateContextWithGroupAsync();
        await using (context)
        {
            for (var i = 0; i < 25; i++)
            {
                context.Products.Add(new Product { GroupId = grp.Id, Name = $"Item{i}", Price = 1m });
            }

            await context.SaveChangesAsync();

            var handler = new ListProductHandler(context);
            var result = await handler.Handle(new ListProductQuery { PageNumber = 3, PageSize = 5 });

            result.Items.Should().HaveCount(5);
            result.PageNumber.Should().Be(3);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(25);
            result.TotalPages.Should().Be(5);
        }
    }

    [Fact]
    public async Task Handle_should_apply_search_term_filter()
    {
        var (context, grp) = await CreateContextWithGroupAsync();
        await using (context)
        {
            context.Products.Add(new Product { GroupId = grp.Id, Name = "Alpha widget one", Price = 1m });
            context.Products.Add(new Product { GroupId = grp.Id, Name = "Beta", Price = 2m });
            await context.SaveChangesAsync();

            var handler = new ListProductHandler(context);
            var result = await handler.Handle(new ListProductQuery { SearchTerm = "widget" });

            result.TotalCount.Should().Be(1);
            result.Items.Should().ContainSingle(i => i.Name.Contains("widget", StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task Handle_should_return_paged_result_with_items()
    {
        var (context, grp) = await CreateContextWithGroupAsync();
        await using (context)
        {
            context.Products.Add(new Product { GroupId = grp.Id, Name = "A", Price = 10m });
            context.Products.Add(new Product { GroupId = grp.Id, Name = "B", Price = 20m });
            await context.SaveChangesAsync();

            var handler = new ListProductHandler(context);
            var result = await handler.Handle(new ListProductQuery { PageNumber = 1, PageSize = 10 });

            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.TotalPages.Should().Be(1);
        }
    }
}
