using Microsoft.FluentUI.AspNetCore.Components;
using Shared.Core;

namespace Client.Components.Base;

public sealed class PagedGridController<TGridItem>
{
    private const int FallbackItemsPerPage = 10;
    private readonly Func<int, int, Task<PagedResult<TGridItem>?>> _fetchPageAsync;
    private readonly Action<IReadOnlyList<TGridItem>, int>? _snapshotChanged;
    private readonly int _itemsPerPage;
    private bool _restoredSnapshotServed;

    public PagedGridController(
        Func<int, int, Task<PagedResult<TGridItem>?>> fetchPageAsync,
        int itemsPerPage,
        IReadOnlyList<TGridItem>? restoredItems = null,
        int restoredTotalCount = 0,
        Action<IReadOnlyList<TGridItem>, int>? snapshotChanged = null)
    {
        _fetchPageAsync = fetchPageAsync ?? throw new ArgumentNullException(nameof(fetchPageAsync));
        _snapshotChanged = snapshotChanged;
        _itemsPerPage = itemsPerPage > 0 ? itemsPerPage : FallbackItemsPerPage;

        Pagination = new PaginationState { ItemsPerPage = _itemsPerPage };

        if (restoredItems != null)
        {
            Items = restoredItems.ToList();
            TotalCount = restoredTotalCount > 0 ? restoredTotalCount : Items.Count;
        }
    }

    public AsyncState<PagedResult<TGridItem>> State { get; } = new();
    public PaginationState Pagination { get; }
    public IReadOnlyList<TGridItem> Items { get; private set; } = [];
    public int TotalCount { get; private set; }
    public bool IsPending => State.IsPending;
    public bool IsError => State.IsError;
    public Exception? Error => State.Error;
    public bool HasItems => Items.Count > 0;

    public async ValueTask<GridItemsProviderResult<TGridItem>> ProvideItemsAsync(GridItemsProviderRequest<TGridItem> request)
    {
        if (Pagination.ItemsPerPage != _itemsPerPage)
        {
            Pagination.ItemsPerPage = _itemsPerPage;
        }

        if (!_restoredSnapshotServed && HasItems)
        {
            _restoredSnapshotServed = true;
            return GridItemsProviderResult.From<TGridItem>(Items.Take(_itemsPerPage).ToList(), TotalCount);
        }

        var pageNumber = (Math.Max(0, request.StartIndex) / _itemsPerPage) + 1;
        var result = await State.RunAsync(() => _fetchPageAsync(pageNumber, _itemsPerPage));

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
