using Microsoft.FluentUI.AspNetCore.Components;
using Shared.Core;

namespace Client.Components.Base;

public sealed class PagedGridController<TGridItem>
{
    private const int FallbackItemsPerPage = 10;
    private readonly Func<int, int, GridItemsProviderRequest<TGridItem>, Task<PagedResult<TGridItem>?>> _fetchPageAsync;
    private readonly Action<IReadOnlyList<TGridItem>, int>? _snapshotChanged;
    private readonly int _itemsPerPage;
    private readonly bool _hasRestoredSnapshot;
    private bool _restoredSnapshotServed;

    public PagedGridController(
        Func<int, int, GridItemsProviderRequest<TGridItem>, Task<PagedResult<TGridItem>?>> fetchPageAsync,
        int itemsPerPage,
        IReadOnlyList<TGridItem>? restoredItems = null,
        int restoredTotalCount = 0,
        Action<IReadOnlyList<TGridItem>, int>? snapshotChanged = null)
    {
        _fetchPageAsync = fetchPageAsync ?? throw new ArgumentNullException(nameof(fetchPageAsync));
        _snapshotChanged = snapshotChanged;
        _itemsPerPage = itemsPerPage > 0 ? itemsPerPage : FallbackItemsPerPage;

        Pagination = new PaginationState { ItemsPerPage = _itemsPerPage };

        if (restoredItems != null && restoredTotalCount > 0)
        {
            Items = restoredItems.ToList();
            TotalCount = restoredTotalCount;
            _hasRestoredSnapshot = true;
        }
    }

    public AsyncState<PagedResult<TGridItem>> State { get; } = new();
    public PaginationState Pagination { get; }
    public IReadOnlyList<TGridItem> Items { get; private set; } = [];
    public int TotalCount { get; private set; }
    private IReadOnlyList<TGridItem> LatestItems => Items.Count > 0
        ? Items
        : (State.Data?.Items?.ToList() ?? []);
    private int LatestTotalCount => TotalCount > 0
        ? TotalCount
        : (State.Data?.TotalCount ?? 0);
    public bool IsPending => State.IsPending;
    public bool IsError => State.IsError;
    public Exception? Error => State.Error;
    public bool HasItems => LatestItems.Count > 0;
    public bool HasNoResults => !IsPending && LatestTotalCount == 0 && !HasItems;
    public bool CanPaginate => LatestTotalCount > _itemsPerPage;

    public async ValueTask<GridItemsProviderResult<TGridItem>> ProvideItemsAsync(GridItemsProviderRequest<TGridItem> request)
    {
        if (Pagination.ItemsPerPage != _itemsPerPage)
        {
            Pagination.ItemsPerPage = _itemsPerPage;
        }

        if (!_restoredSnapshotServed && _hasRestoredSnapshot && HasItems)
        {
            _restoredSnapshotServed = true;
            return GridItemsProviderResult.From<TGridItem>(Items.Take(_itemsPerPage).ToList(), TotalCount);
        }

        var pageNumber = (Math.Max(0, request.StartIndex) / _itemsPerPage) + 1;
        var result = await State.RunAsync(() => _fetchPageAsync(pageNumber, _itemsPerPage, request));

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
