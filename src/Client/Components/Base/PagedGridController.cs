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
    public const int DefaultItemsPerPage = 10;
    public static IReadOnlyList<int> SupportedItemsPerPageOptions { get; } = [10, 20, 50, 100];

    private readonly Func<int, int, GridItemsProviderRequest<TGridItem>, Task<PagedResult<TGridItem>?>> _fetchPageAsync;
    private readonly Action<IReadOnlyList<TGridItem>, int>? _onSnapshotChanged;
    private bool _restoredSnapshotServed;

    public PagedGridController(
        Func<int, int, GridItemsProviderRequest<TGridItem>, Task<PagedResult<TGridItem>?>> fetchPageAsync,
        IReadOnlyList<TGridItem>? restoredItems = null,
        int restoredTotalCount = 0,
        Action<IReadOnlyList<TGridItem>, int>? snapshotChanged = null)
    {
        _fetchPageAsync = fetchPageAsync ?? throw new ArgumentNullException(nameof(fetchPageAsync));
        _onSnapshotChanged = snapshotChanged;
        Pagination = new PaginationState { ItemsPerPage = DefaultItemsPerPage };

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
    public bool CanPaginate => TotalCount > GetValidatedItemsPerPage(Pagination.ItemsPerPage);

    public async ValueTask<GridItemsProviderResult<TGridItem>> ProvideItemsAsync(GridItemsProviderRequest<TGridItem> request)
    {
        var pageSize = GetValidatedItemsPerPage(Pagination.ItemsPerPage);
        if (Pagination.ItemsPerPage != pageSize)
        {
            Pagination.ItemsPerPage = pageSize;
        }

        if (!_restoredSnapshotServed && HasItems)
        {
            _restoredSnapshotServed = true;
            return GridItemsProviderResult.From(Items.Take(pageSize).ToList(), TotalCount);
        }

        var pageNumber = (Math.Max(0, request.StartIndex) / pageSize) + 1;

        await State.RunAsync(async () =>
        {
            var result = await _fetchPageAsync(pageNumber, pageSize, request);
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

    private static int GetValidatedItemsPerPage(int value)
    {
        if (SupportedItemsPerPageOptions.Contains(value))
        {
            return value;
        }

        return DefaultItemsPerPage;
    }
}
