using Shared.Core;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Shared.Core.CRUD;

namespace Client.Components.Base;

public abstract class PagedListComponent<TResponse, TQuery> : BaseComponent
    where TQuery : IRequest<PagedResult<TResponse>>, IPageableQuery, new()
{
    private bool _restoredItemsServed;
    private const int FallbackItemsPerPage = 10;

    protected virtual int ItemsPerPage => 10;
    protected PaginationState Pagination { get; } = new();

    [PersistentState] public List<TResponse>? Items { get; set; }
    protected TQuery Query { get; set; } = new();
    [PersistentState] public int TotalCount { get; set; }
    protected int EffectiveItemsPerPage => ItemsPerPage > 0 ? ItemsPerPage : FallbackItemsPerPage;

    protected override async Task OnInitializedAsync()
    {
        Pagination.ItemsPerPage = EffectiveItemsPerPage;

        if (Items == null)
        {
            await LoadDataAsync();
        }
    }

    protected virtual async Task LoadDataAsync()
    {
        var pageSize = EffectiveItemsPerPage;
        Query.PageNumber = Query.PageNumber.GetValueOrDefault(1);
        if (Query.PageNumber <= 0)
        {
            Query.PageNumber = 1;
        }
        Query.PageSize = pageSize;

        var result = await SendAsync(Query);
        if (result != null)
        {
            Items = result.Items?.ToList() ?? new List<TResponse>();
            TotalCount = result.TotalCount;
        }
    }

    protected ValueTask<GridItemsProviderResult<TResponse>> ProvideItemsAsync(GridItemsProviderRequest<TResponse> request)
    {
        // Keep page-level override authoritative over incoming grid request count.
        var pageSize = EffectiveItemsPerPage;
        if (Pagination.ItemsPerPage != pageSize)
        {
            Pagination.ItemsPerPage = pageSize;
        }

        var startIndex = Math.Max(0, request.StartIndex);
        Query.PageNumber = (startIndex / pageSize) + 1;
        Query.PageSize = pageSize;

        if (!_restoredItemsServed && Items != null)
        {
            _restoredItemsServed = true;
            var restoredTotalCount = TotalCount > 0 ? TotalCount : Items.Count;
            var restoredItems = Items.Take(pageSize).ToList();
            return ValueTask.FromResult(GridItemsProviderResult.From(restoredItems, restoredTotalCount));
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
