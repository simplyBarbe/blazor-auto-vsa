using Shared.Core;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Linq;
using Shared.Core.CRUD;

namespace Client.Components.Base;

/// <summary>
/// Base class for list components that handle pagination and data loading.
/// </summary>
/// <typeparam name="TResponse">The type of items in the list.</typeparam>
/// <typeparam name="TQuery">The type of the query used to fetch data.</typeparam>
public abstract class PagedListComponent<TResponse, TQuery> : BaseComponent
    where TQuery : IRequest<PagedResult<TResponse>>, IPageableQuery, new()
{
    /// <summary>
    /// Default page size for list views.
    /// </summary>
    protected virtual int ItemsPerPage => 10;

    /// <summary>
    /// Pagination state for data grids.
    /// </summary>
    protected PaginationState Pagination { get; } = new();

    /// <summary>
    /// The current set of items to display.
    /// </summary>
    protected IQueryable<TResponse>? Items { get; set; }

    /// <summary>
    /// The query used for data loading, including pagination and filtering parameters.
    /// </summary>
    protected TQuery Query { get; set; } = new();

    /// <summary>
    /// The total count of items across all pages.
    /// </summary>
    protected int TotalCount { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        Pagination.ItemsPerPage = ItemsPerPage;

        await LoadDataAsync();
    }

    /// <summary>
    /// Loads data using the current Query parameters.
    /// </summary>
    protected virtual async Task LoadDataAsync()
    {
        var result = await SendAsync(Query);
        if (result != null)
        {
            Items = result.Items.AsQueryable();
            TotalCount = result.TotalCount;
        }
    }

    /// <summary>
    /// Shared items provider for data grids with paging.
    /// </summary>
    protected async ValueTask<GridItemsProviderResult<TResponse>> ProvideItemsAsync(
        GridItemsProviderRequest<TResponse> request)
    {
        var pageSize = request.Count.GetValueOrDefault(Pagination.ItemsPerPage);
        if (pageSize <= 0)
        {
            pageSize = Pagination.ItemsPerPage;
        }

        var startIndex = Math.Max(0, request.StartIndex);
        Query.PageNumber = (startIndex / pageSize) + 1;
        Query.PageSize = pageSize;

        var result = await SendAsync(Query, options: new RequestOptions(TrackLoading: false));

        if (result == null)
        {
            return GridItemsProviderResult.From(Array.Empty<TResponse>(), TotalCount);
        }

        var items = result.Items?.ToList() ?? new List<TResponse>();
        Items = items.AsQueryable();
        TotalCount = result.TotalCount;
        return GridItemsProviderResult.From(items, result.TotalCount);
    }
}
