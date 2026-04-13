using Client.Components.Base;
using FluentAssertions;
using Microsoft.FluentUI.AspNetCore.Components;
using Shared.Core;
using System.Reflection;

namespace Unit.Features.Components;

public class PagedGridControllerTests
{
    [Fact]
    public async Task ProvideItemsAsync_should_translate_request_into_page_number_and_page_size()
    {
        var capturedPageNumber = 0;
        var capturedPageSize = 0;

        var controller = CreateController<int>(
            async (pageNumber, pageSize) =>
            {
                capturedPageNumber = pageNumber;
                capturedPageSize = pageSize;
                return await Task.FromResult(new PagedResult<int>
                {
                    Items = [11, 12, 13],
                    TotalCount = 25,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });
            },
            itemsPerPage: 5);

        var result = await controller.ProvideItemsAsync(CreateRequest<int>(startIndex: 10, count: 5));

        capturedPageNumber.Should().Be(3);
        capturedPageSize.Should().Be(5);
        result.Items.Should().Equal(11, 12, 13);
        result.TotalItemCount.Should().Be(25);
        controller.Items.Should().Equal(11, 12, 13);
        controller.TotalCount.Should().Be(25);
    }

    [Fact]
    public async Task ProvideItemsAsync_should_serve_restored_snapshot_once_before_fetching_live_data()
    {
        var fetchCalls = 0;

        var controller = CreateController<int>(
            async (_, _) =>
            {
                fetchCalls++;
                return await Task.FromResult(new PagedResult<int>
                {
                    Items = [9, 10],
                    TotalCount = 10,
                    PageNumber = 1,
                    PageSize = 5
                });
            },
            itemsPerPage: 5,
            restoredItems: [1, 2, 3, 4, 5],
            restoredTotalCount: 10);

        var restored = await controller.ProvideItemsAsync(CreateRequest<int>(startIndex: 0, count: 5));
        var live = await controller.ProvideItemsAsync(CreateRequest<int>(startIndex: 0, count: 5));

        fetchCalls.Should().Be(1);
        restored.Items.Should().Equal(1, 2, 3, 4, 5);
        restored.TotalItemCount.Should().Be(10);
        live.Items.Should().Equal(9, 10);
        live.TotalItemCount.Should().Be(10);
    }

    [Fact]
    public async Task ProvideItemsAsync_should_keep_existing_snapshot_when_fetch_fails()
    {
        var controller = CreateController<int>(
            (_, _) => Task.FromException<PagedResult<int>?>(new InvalidOperationException("boom")),
            itemsPerPage: 5,
            restoredItems: [1, 2],
            restoredTotalCount: 2);

        await controller.ProvideItemsAsync(CreateRequest<int>(startIndex: 0, count: 5));
        var failed = await controller.ProvideItemsAsync(CreateRequest<int>(startIndex: 0, count: 5));

        controller.IsError.Should().BeTrue();
        controller.Error.Should().BeOfType<InvalidOperationException>();
        failed.Items.Should().Equal(1, 2);
        failed.TotalItemCount.Should().Be(2);
        controller.Items.Should().Equal(1, 2);
    }

    [Fact]
    public async Task RefreshAsync_should_raise_refresh_request_without_reset()
    {
        var controller = CreateController<int>((_, _) => Task.FromResult<PagedResult<int>?>(null), itemsPerPage: 5);
        var resetToFirstPage = new List<bool>();

        controller.RefreshRequested += reset => resetToFirstPage.Add(reset);

        await controller.RefreshAsync();

        resetToFirstPage.Should().Equal(false);
    }

    [Fact]
    public async Task ResetAndRefreshAsync_should_raise_refresh_request_with_reset()
    {
        var controller = CreateController<int>((_, _) => Task.FromResult<PagedResult<int>?>(null), itemsPerPage: 5);
        var resetToFirstPage = new List<bool>();

        controller.RefreshRequested += reset => resetToFirstPage.Add(reset);

        await controller.ResetAndRefreshAsync();

        resetToFirstPage.Should().Equal(true);
    }

    private static PagedGridController<T> CreateController<T>(
        Func<int, int, Task<PagedResult<T>?>> fetchPageAsync,
        int itemsPerPage,
        IReadOnlyList<T>? restoredItems = null,
        int restoredTotalCount = 0)
    {
        return new PagedGridController<T>(
            new QueryResult<PagedResult<T>>(),
            fetchPageAsync,
            itemsPerPage,
            restoredItems,
            restoredTotalCount);
    }

    private static GridItemsProviderRequest<T> CreateRequest<T>(int startIndex, int count)
    {
        var constructor = typeof(GridItemsProviderRequest<T>).GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .Single();

        var arguments = constructor.GetParameters()
            .Select(parameter => parameter.Name switch
            {
                "startIndex" => (object)startIndex,
                "count" => count,
                "sortByColumn" => null!,
                "sortByAscending" => true,
                "cancellationToken" => CancellationToken.None,
                _ => throw new InvalidOperationException($"Unsupported GridItemsProviderRequest parameter '{parameter.Name}'.")
            })
            .ToArray();

        return (GridItemsProviderRequest<T>)constructor.Invoke(arguments);
    }
}
