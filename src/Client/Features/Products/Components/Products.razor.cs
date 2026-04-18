using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Shared.Core;
using Shared.Features.Products.Responses;
using Shared.Features.Products.Update;
using Shared.Features.Products.Delete;
using Shared.Features.Products.List;
using Client.Components.Base;

namespace Client.Features.Products.Components;

public partial class ProductsBase : BaseComponent
{
    private const int ItemsPerPage = 5;

    protected bool IsBrowser => OperatingSystem.IsBrowser();
    protected ListProductQuery Query { get; } = new();
    protected PagedGridController<ProductResponse> GridController { get; private set; } = default!;
    protected PagedDataGrid<ProductResponse>? Grid { get; set; }
    private readonly AsyncState _delete = new();
    private int? _activeDeleteId;

    [PersistentState]
    public List<ProductResponse>? RestoredItems { get; set; }

    [PersistentState]
    public int RestoredTotalCount { get; set; }

    protected override void OnInitialized()
    {
        GridController = new PagedGridController<ProductResponse>(
            FetchProductsAsync,
            ItemsPerPage,
            RestoredItems,
            RestoredTotalCount,
            (items, totalCount) =>
            {
                RestoredItems = items.ToList();
                RestoredTotalCount = totalCount;
            });

        Track(GridController.State);
        Track(_delete);
    }

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrWhiteSpace(Query.SearchTerm))
        {
            Query.SearchTerm = "";
        }
        await base.OnInitializedAsync();
    }

    protected async Task OnApplyFilterAsync()
    {
        if (Grid != null) await Grid.RefreshAsync(resetToFirstPage: true);
    }

    private Task<PagedResult<ProductResponse>?> FetchProductsAsync(
        int pageNumber,
        int pageSize,
        GridItemsProviderRequest<ProductResponse> gridRequest)
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

    protected async Task OnAddProductAsync()
    {
        await ShowProductDialogAndRefreshAsync(new UpdateProductCommand(), "Add Product");
    }

    protected async Task OnEditProductAsync(ProductResponse product)
    {
        var command = new UpdateProductCommand(product.Id, product.Name, product.Price);
        await ShowProductDialogAndRefreshAsync(command, "Edit Product");
    }

    protected async Task OnDeleteProductAsync(ProductResponse product)
    {
        if (_delete.IsPending) return;

        var confirmed = await ConfirmAsync($"Are you sure you want to delete {product.Name}?");
        if (!confirmed) return;

        _activeDeleteId = product.Id;
        await InvokeAsync(StateHasChanged);

        var success = await _delete.RunAsync(() =>
            SendAsync(new DeleteProductCommand(product.Id),
                options: new RequestOptions(SuccessMessage: "Product deleted!")));

        _activeDeleteId = null;

        if (success && Grid != null)
        {
            await Grid.RefreshAsync();
        }
    }

    protected bool IsDeletePending(ProductResponse product) => _delete.IsPending && _activeDeleteId == product.Id;

    private async Task ShowProductDialogAndRefreshAsync(UpdateProductCommand command, string title)
    {
        var dialog = await DialogService.ShowDialogAsync<ProductDialog>(command, new DialogParameters
        {
            Title = title,
            Width = "400px",
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
