using Microsoft.FluentUI.AspNetCore.Components;
using Shared.Core;

namespace Client.Components.Base;

public sealed class PagedGridController<TGridItem>
{
    private const int FallbackItemsPerPage = 10;
    private readonly Func<int, int, Task<PagedResult<TGridItem>?>> _fetchPageAsync;
    private readonly Action<IReadOnlyList<TGridItem>, int>? _snapshotChanged;
    private readonly int _itemsPerPage;
    private bool _restoredItemsServed;

    public PagedGridController(
        QueryResult<PagedResult<TGridItem>> queryResult,
        Func<int, int, Task<PagedResult<TGridItem>?>> fetchPageAsync,
        int itemsPerPage,
        IReadOnlyList<TGridItem>? restoredItems = null,
        int restoredTotalCount = 0,
        Action<IReadOnlyList<TGridItem>, int>? snapshotChanged = null)
    {
        QueryResult = queryResult ?? throw new ArgumentNullException(nameof(queryResult));
        _fetchPageAsync = fetchPageAsync ?? throw new ArgumentNullException(nameof(fetchPageAsync));
        _snapshotChanged = snapshotChanged;
        _itemsPerPage = itemsPerPage > 0 ? itemsPerPage : FallbackItemsPerPage;

        Pagination = new PaginationState
        {
            ItemsPerPage = _itemsPerPage
        };

        if (restoredItems != null)
        {
            Items = restoredItems.ToList();
            TotalCount = restoredTotalCount > 0 ? restoredTotalCount : Items.Count;
        }
    }

    public QueryResult<PagedResult<TGridItem>> QueryResult { get; }
    public PaginationState Pagination { get; }
    public IReadOnlyList<TGridItem> Items { get; private set; } = [];
    public int TotalCount { get; private set; }
    public bool IsPending => QueryResult.IsPending;
    public bool IsError => QueryResult.IsError;
    public Exception? Error => QueryResult.Error;
    public bool HasItems => Items.Count > 0;

    public event Action<bool>? RefreshRequested;

    public Task RefreshAsync()
    {
        RefreshRequested?.Invoke(false);
        return Task.CompletedTask;
    }

    public Task ResetAndRefreshAsync()
    {
        RefreshRequested?.Invoke(true);
        return Task.CompletedTask;
    }

    public ValueTask<GridItemsProviderResult<TGridItem>> ProvideItemsAsync(GridItemsProviderRequest<TGridItem> request)
    {
        var pageSize = _itemsPerPage;
        if (Pagination.ItemsPerPage != pageSize)
        {
            Pagination.ItemsPerPage = pageSize;
        }

        if (!_restoredItemsServed && HasItems)
        {
            _restoredItemsServed = true;
            return ValueTask.FromResult(GridItemsProviderResult.From<TGridItem>(Items.Take(pageSize).ToList(), TotalCount));
        }

        return ProvideItemsFromRequestAsync();

        async ValueTask<GridItemsProviderResult<TGridItem>> ProvideItemsFromRequestAsync()
        {
            var startIndex = Math.Max(0, request.StartIndex);
            var pageNumber = (startIndex / pageSize) + 1;

            var result = await QueryResult.ExecuteAsync(() => _fetchPageAsync(pageNumber, pageSize));

            if (result == null)
            {
                return GridItemsProviderResult.From<TGridItem>(Items.ToList(), TotalCount);
            }

            Items = result.Items.ToList();
            TotalCount = result.TotalCount;
            _snapshotChanged?.Invoke(Items, TotalCount);

            return GridItemsProviderResult.From<TGridItem>(Items.ToList(), result.TotalCount);
        }
    }
}
