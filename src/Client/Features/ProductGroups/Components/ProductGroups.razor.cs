using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Shared.Core;
using Shared.Features.ProductGroups.Delete;
using Shared.Features.ProductGroups.List;
using Shared.Features.ProductGroups.Responses;
using Shared.Features.ProductGroups.Update;
using Client.Components.Base;

namespace Client.Features.ProductGroups.Components;

public partial class ProductGroupsBase : BaseComponent
{
    private const int ItemsPerPage = 10;

    protected ListProductGroupQuery Query { get; } = new();
    protected PagedGridController<ProductGroupResponse> GridController { get; private set; } = default!;
    protected PagedDataGrid<ProductGroupResponse>? Grid { get; set; }
    private AsyncState _delete = default!;
    private int? _activeDeleteId;

    [PersistentState]
    public List<ProductGroupResponse>? RestoredItems { get; set; }

    [PersistentState]
    public int RestoredTotalCount { get; set; }

    protected override void OnInitialized()
    {
        GridController = new PagedGridController<ProductGroupResponse>(
            FetchAsync,
            ItemsPerPage,
            RestoredItems,
            RestoredTotalCount,
            (items, totalCount) =>
            {
                RestoredItems = items.ToList();
                RestoredTotalCount = totalCount;
            });

        Track(GridController.State);
        _delete = UseAsyncState();
    }

    protected async Task OnApplyFilterAsync()
    {
        if (Grid != null)
        {
            await Grid.RefreshAsync(resetToFirstPage: true);
        }
    }

    private Task<PagedResult<ProductGroupResponse>?> FetchAsync(
        int pageNumber,
        int pageSize,
        GridItemsProviderRequest<ProductGroupResponse> gridRequest)
    {
        Query.PageNumber = pageNumber;
        Query.PageSize = pageSize;
        GridItemsProviderRequestSort.Apply(gridRequest, (sortBy, ascending) =>
        {
            Query.SortBy = sortBy;
            Query.SortAscending = ascending;
        });
        return SendAsync(Query);
    }

    protected async Task OnAddAsync()
    {
        await ShowDialogAsync(new UpdateProductGroupCommand(), "Add Group");
    }

    protected async Task OnEditAsync(ProductGroupResponse item)
    {
        await ShowDialogAsync(new UpdateProductGroupCommand(item.Id, item.CategoryId, item.Name), "Edit Group");
    }

    protected async Task OnDeleteAsync(ProductGroupResponse item)
    {
        if (_delete.IsPending)
        {
            return;
        }

        var confirmed = await ConfirmAsync($"Delete group {item.Name}?");
        if (!confirmed)
        {
            return;
        }

        _activeDeleteId = item.Id;
        await InvokeAsync(StateHasChanged);

        var success = await _delete.RunAsync(() =>
            SendAsync(new DeleteProductGroupCommand(item.Id),
                options: new RequestOptions(SuccessMessage: "Group deleted.")));

        _activeDeleteId = null;

        if (success && Grid != null)
        {
            await Grid.RefreshAsync();
        }
    }

    protected bool IsDeletePending(ProductGroupResponse item) =>
        _delete.IsPending && _activeDeleteId == item.Id;

    private async Task ShowDialogAsync(UpdateProductGroupCommand command, string title)
    {
        var dialog = await DialogService.ShowDialogAsync<ProductGroupDialog>(command, new DialogParameters
        {
            Title = title,
            Width = "480px",
            TrapFocus = true,
            Modal = true,
            PreventScroll = true
        });

        var result = await dialog.Result;
        if (!result.Cancelled && Grid != null)
        {
            await Grid.RefreshAsync();
        }
    }
}
