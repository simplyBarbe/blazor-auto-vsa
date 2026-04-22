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

        var controller = new PagedGridController<int>(
            async (pageNumber, pageSize, _) =>
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
            });

        controller.Pagination.ItemsPerPage = 20;

        var result = await controller.ProvideItemsAsync(CreateRequest<int>(startIndex: 40, count: 20));

        capturedPageNumber.Should().Be(3);
        capturedPageSize.Should().Be(20);
        result.Items.Should().Equal(11, 12, 13);
        result.TotalItemCount.Should().Be(25);
        controller.Items.Should().Equal(11, 12, 13);
        controller.TotalCount.Should().Be(25);
    }

    [Fact]
    public async Task ProvideItemsAsync_should_serve_restored_snapshot_once_before_fetching_live_data()
    {
        var fetchCalls = 0;

        var controller = new PagedGridController<int>(
            async (_, _, _) =>
            {
                fetchCalls++;
                return await Task.FromResult(new PagedResult<int>
                {
                    Items = [9, 10],
                    TotalCount = 10,
                    PageNumber = 1,
                    PageSize = 20
                });
            },
            restoredItems: [1, 2, 3, 4, 5],
            restoredTotalCount: 10);

        controller.Pagination.ItemsPerPage = 20;

        var restored = await controller.ProvideItemsAsync(CreateRequest<int>(startIndex: 0, count: 20));
        var live = await controller.ProvideItemsAsync(CreateRequest<int>(startIndex: 0, count: 20));

        fetchCalls.Should().Be(1);
        restored.Items.Should().Equal(1, 2, 3, 4, 5);
        restored.TotalItemCount.Should().Be(10);
        live.Items.Should().Equal(9, 10);
        live.TotalItemCount.Should().Be(10);
    }

    [Fact]
    public async Task ProvideItemsAsync_should_keep_existing_snapshot_when_fetch_fails()
    {
        var controller = new PagedGridController<int>(
            (_, _, _) => Task.FromException<PagedResult<int>?>(new InvalidOperationException("boom")),
            restoredItems: [1, 2],
            restoredTotalCount: 2);

        controller.Pagination.ItemsPerPage = 20;

        await controller.ProvideItemsAsync(CreateRequest<int>(startIndex: 0, count: 20));
        var failed = await controller.ProvideItemsAsync(CreateRequest<int>(startIndex: 0, count: 20));

        controller.IsError.Should().BeTrue();
        controller.Error.Should().BeOfType<InvalidOperationException>();
        failed.Items.Should().Equal(1, 2);
        failed.TotalItemCount.Should().Be(2);
        controller.Items.Should().Equal(1, 2);
    }

    [Fact]
    public async Task ProvideItemsAsync_should_fallback_to_default_page_size_for_unsupported_values()
    {
        var capturedPageSize = 0;

        var controller = new PagedGridController<int>(
            async (_, pageSize, _) =>
            {
                capturedPageSize = pageSize;
                return await Task.FromResult(new PagedResult<int>
                {
                    Items = [1, 2, 3],
                    TotalCount = 3,
                    PageNumber = 1,
                    PageSize = pageSize
                });
            });

        controller.Pagination.ItemsPerPage = 7;

        _ = await controller.ProvideItemsAsync(CreateRequest<int>(startIndex: 0, count: 10));

        capturedPageSize.Should().Be(PagedGridController<int>.DefaultItemsPerPage);
        controller.Pagination.ItemsPerPage.Should().Be(PagedGridController<int>.DefaultItemsPerPage);
    }

    private static GridItemsProviderRequest<T> CreateRequest<T>(int startIndex, int count)
    {
        var constructor = typeof(GridItemsProviderRequest<T>)
            .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .OrderBy(c => c.GetParameters().Length)
            .First();

        var arguments = constructor.GetParameters()
            .Select(parameter => parameter.Name switch
            {
                "startIndex" => (object)startIndex,
                "count" => count,
                "sortByColumn" => null!,
                "sortByAscending" => true,
                "cancellationToken" => CancellationToken.None,
                _ => parameter.HasDefaultValue ? parameter.DefaultValue! : GetDefaultValue(parameter.ParameterType)!
            })
            .ToArray();

        return (GridItemsProviderRequest<T>)constructor.Invoke(arguments);
    }

    private static object? GetDefaultValue(Type type)
        => type.IsValueType ? Activator.CreateInstance(type) : null;
}
