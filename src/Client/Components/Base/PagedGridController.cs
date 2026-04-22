using Microsoft.FluentUI.AspNetCore.Components;
using Shared.Core;

namespace Client.Components.Base;

/// <summary>
/// Paged grid state holder used with <see cref="PagedDataGrid{TGridItem}"/>.
/// Single source of truth: <see cref="Items"/>/<see cref="TotalCount"/> are always
/// consistent when <see cref="State"/>.OnChange fires, because they are updated
/// inside the fetch callback before <see cref="AsyncState{T}"/> flips its pending flag.
/// </summary>
public sealed class PagedGridController<TGridItem>
{
    private const int FallbackItemsPerPage = 10;

    private readonly Func<int, int, GridItemsProviderRequest<TGridItem>, Task<PagedResult<TGridItem>?>> _fetchPageAsync;
    private readonly Action<IReadOnlyList<TGridItem>, int>? _onSnapshotChanged;
    private readonly int _itemsPerPage;
    private bool _restoredSnapshotServed;

    public PagedGridController(
        Func<int, int, GridItemsProviderRequest<TGridItem>, Task<PagedResult<TGridItem>?>> fetchPageAsync,
        int itemsPerPage,
        IReadOnlyList<TGridItem>? restoredItems = null,
        int restoredTotalCount = 0,
        Action<IReadOnlyList<TGridItem>, int>? snapshotChanged = null)
    {
        _fetchPageAsync = fetchPageAsync ?? throw new ArgumentNullException(nameof(fetchPageAsync));
        _onSnapshotChanged = snapshotChanged;
        _itemsPerPage = itemsPerPage > 0 ? itemsPerPage : FallbackItemsPerPage;
        Pagination = new PaginationState { ItemsPerPage = _itemsPerPage };

        if (restoredItems is { Count: > 0 } && restoredTotalCount > 0)
        {
            Items = restoredItems.ToList();
            TotalCount = restoredTotalCount;
        }
        else
        {
            _restoredSnapshotServed = true;
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
    public bool HasNoResults => !IsPending && !HasItems && TotalCount == 0;
    public bool CanPaginate => TotalCount > _itemsPerPage;

    public async ValueTask<GridItemsProviderResult<TGridItem>> ProvideItemsAsync(GridItemsProviderRequest<TGridItem> request)
    {
        if (Pagination.ItemsPerPage != _itemsPerPage)
        {
            Pagination.ItemsPerPage = _itemsPerPage;
        }

        if (!_restoredSnapshotServed && HasItems)
        {
            _restoredSnapshotServed = true;
            return GridItemsProviderResult.From(Items.Take(_itemsPerPage).ToList(), TotalCount);
        }

        var pageNumber = (Math.Max(0, request.StartIndex) / _itemsPerPage) + 1;

        await State.RunAsync(async () =>
        {
            var result = await _fetchPageAsync(pageNumber, _itemsPerPage, request);
            if (result is not null)
            {
                Items = result.Items.ToList();
                TotalCount = result.TotalCount;
                _onSnapshotChanged?.Invoke(Items, TotalCount);
            }
            return result;
        });

        return GridItemsProviderResult.From(Items.ToList(), TotalCount);
    }
}
