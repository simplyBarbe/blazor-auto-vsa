using Microsoft.FluentUI.AspNetCore.Components;
using Shared.Features.Products.Responses;
using Shared.Features.Products.Update;
using Shared.Features.Products.Delete;
using Shared.Features.Products.List;
using Client.Components.Base;

namespace Client.Features.Products.Components;

public partial class ProductsBase : PagedListComponent<ProductResponse, ListProductQuery>
{
    protected bool IsBrowser => OperatingSystem.IsBrowser();

    protected FluentDataGrid<ProductResponse>? _grid;

    protected override int ItemsPerPage => 5;

    protected override async Task LoadDataAsync()
    {
        if (_grid != null)
        {
            await _grid.RefreshDataAsync();
        }
        else
        {
            await base.LoadDataAsync();
        }
    }

    protected async ValueTask<GridItemsProviderResult<ProductResponse>> ProductProvider(GridItemsProviderRequest<ProductResponse> request)
    {
        return await ProvideItemsAsync(request);
    }

    protected async Task OnApplyFilterAsync()
    {
        Query.PageNumber = 1;
        await LoadDataAsync();
    }

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrWhiteSpace(Query.SearchTerm))
        {
            Query.SearchTerm = "";
        }
        await base.OnInitializedAsync();
    }

    protected async Task OnAddProductAsync()
    {
        if (IsLoading) return;

        await ShowProductDialogAndRefreshAsync(new UpdateProductCommand(), "Add Product");
    }

    protected async Task OnEditProductAsync(ProductResponse product)
    {
        if (IsLoading) return;

        var command = new UpdateProductCommand(product.Id, product.Name, product.Price);
        await ShowProductDialogAndRefreshAsync(command, "Edit Product");
    }

    protected async Task OnDeleteProductAsync(ProductResponse product)
    {
        if (IsLoading) return;

        var confirmed = await ConfirmAsync($"Are you sure you want to delete {product.Name}?");
        if (confirmed)
        {
            await SendAsync(
                new DeleteProductCommand(product.Id),
                options: new RequestOptions(SuccessMessage: "Product deleted!"));
            await LoadDataAsync();
        }
    }

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
        if (!result.Cancelled)
        {
            await LoadDataAsync();
        }
    }
}
