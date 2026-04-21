using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Shared.Core;
using Shared.Features.Categories.Delete;
using Shared.Features.Categories.List;
using Shared.Features.Categories.Responses;
using Shared.Features.Categories.Update;
using Client.Components.Base;

namespace Client.Features.Categories.Components;

public partial class CategoriesBase : BaseComponent
{
    private const int ItemsPerPage = 10;

    protected ListCategoryQuery Query { get; } = new();
    protected PagedGridController<CategoryResponse> GridController { get; private set; } = default!;
    protected PagedDataGrid<CategoryResponse>? Grid { get; set; }
    private AsyncState _delete = default!;
    private int? _activeDeleteId;

    [PersistentState]
    public List<CategoryResponse>? RestoredItems { get; set; }

    [PersistentState]
    public int RestoredTotalCount { get; set; }

    protected override void OnInitialized()
    {
        GridController = new PagedGridController<CategoryResponse>(
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

    private Task<PagedResult<CategoryResponse>?> FetchAsync(
        int pageNumber,
        int pageSize,
        GridItemsProviderRequest<CategoryResponse> gridRequest)
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
        await ShowDialogAsync(new UpdateCategoryCommand(), "Add Category");
    }

    protected async Task OnEditAsync(CategoryResponse item)
    {
        await ShowDialogAsync(new UpdateCategoryCommand(item.Id, item.Name), "Edit Category");
    }

    protected async Task OnDeleteAsync(CategoryResponse item)
    {
        if (_delete.IsPending)
        {
            return;
        }

        var confirmed = await ConfirmAsync($"Delete category {item.Name}?");
        if (!confirmed)
        {
            return;
        }

        _activeDeleteId = item.Id;
        await InvokeAsync(StateHasChanged);

        var success = await _delete.RunAsync(() =>
            SendAsync(new DeleteCategoryCommand(item.Id),
                options: new RequestOptions(SuccessMessage: "Category deleted.")));

        _activeDeleteId = null;

        if (success && Grid != null)
        {
            await Grid.RefreshAsync();
        }
    }

    protected bool IsDeletePending(CategoryResponse item) =>
        _delete.IsPending && _activeDeleteId == item.Id;

    private async Task ShowDialogAsync(UpdateCategoryCommand command, string title)
    {
        var dialog = await DialogService.ShowDialogAsync<CategoryDialog>(command, new DialogParameters
        {
            Title = title,
            Width = "420px",
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
