using Shared.Core;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Linq;
using Microsoft.AspNetCore.Components;
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
    private bool _restoredItemsServed;

    /// <summary>
    /// Default page size for list views.
    /// </summary>
    protected virtual int ItemsPerPage => 10;

    /// <summary>
    /// Pagination state for data grids.
    /// </summary>
    protected PaginationState Pagination { get; } = new();

    [PersistentState] public List<TResponse>? Items { get; set; }

    /// <summary>
    /// The query used for data loading, including pagination and filtering parameters.
    /// </summary>
    protected TQuery Query { get; set; } = new();

    /// <summary>
    /// The total count of items across all pages.
    /// </summary>
    [PersistentState] public int TotalCount { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Pagination.ItemsPerPage = ItemsPerPage;

        if (Items == null)
        {
            await LoadDataAsync();
        }
    }

    /// <summary>
    /// Loads data using the current Query parameters.
    /// </summary>
    protected virtual async Task LoadDataAsync()
    {
        var result = await SendAsync(Query);
        if (result != null)
        {
            Items = result.Items?.ToList() ?? new List<TResponse>();
            TotalCount = result.TotalCount;
        }
    }

    /// <summary>
    /// Shared items provider for data grids with paging.
    /// </summary>
    protected ValueTask<GridItemsProviderResult<TResponse>> ProvideItemsAsync(GridItemsProviderRequest<TResponse> request)
    {
        var pageSize = request.Count.GetValueOrDefault(Pagination.ItemsPerPage);
        if (pageSize <= 0)
        {
            pageSize = ItemsPerPage;
        }

        var startIndex = Math.Max(0, request.StartIndex);
        Query.PageNumber = (startIndex / pageSize) + 1;
        Query.PageSize = pageSize;

        // Hydration path: serve restored state synchronously once to avoid the grid loading flash.
        if (!_restoredItemsServed && Items != null)
        {
            _restoredItemsServed = true;
            var restoredTotalCount = TotalCount > 0 ? TotalCount : Items.Count;
            return ValueTask.FromResult(GridItemsProviderResult.From(Items, restoredTotalCount));
        }

        return ProvideItemsFromRequestAsync();

        async ValueTask<GridItemsProviderResult<TResponse>> ProvideItemsFromRequestAsync()
        {
            var result = await SendAsync(Query, options: new RequestOptions(TrackLoading: false));

            if (result == null)
            {
                return GridItemsProviderResult.From(Array.Empty<TResponse>(), TotalCount);
            }

            var items = result.Items?.ToList() ?? new List<TResponse>();
            Items = items;
            TotalCount = result.TotalCount;
            return GridItemsProviderResult.From(items, result.TotalCount);
        }
    }
}
