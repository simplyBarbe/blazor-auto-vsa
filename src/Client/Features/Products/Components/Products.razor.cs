using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using Shared.Core;
using Shared.Features.Products.Responses;
using Shared.Features.Products.Create;
using Shared.Features.Products.Update;
using Shared.Features.Products.Delete;
using Shared.Features.Products.List;
using Shared.Features.Products.Get;
using Shared.Core.Validation;
using Client.Extensions;

using Client.Components.Base;

namespace Client.Features.Products.Components;

public partial class Products : PagedListComponent<ProductResponse, ListProductQuery>
{
    protected bool IsBrowser => OperatingSystem.IsBrowser();
    
    private FluentDataGrid<ProductResponse>? _grid;

    protected override int ItemsPerPage => 3;

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

    private async ValueTask<GridItemsProviderResult<ProductResponse>> ProductProvider(GridItemsProviderRequest<ProductResponse> request)
    {
        return await ProvideItemsAsync(request);
    }

    private async Task LoadProductsAsync() => await LoadDataAsync();

    private async Task OnAddProductAsync()
    {
        if (IsLoading) return;

        var command = new UpdateProductCommand();
        var dialog = await DialogService.ShowDialogAsync<ProductDialog>(new DialogOptions
        {
            Header = { Title = "Add Product" },
            Width = "400px",
            Modal = true,
            Parameters = new Dictionary<string, object?>
            {
                ["Content"] = command
            }
        });

        if (!dialog.Cancelled)
        {
            await LoadProductsAsync();
        }
    }

    private async Task OnEditProductAsync(ProductResponse product)
    {
        if (IsLoading) return;

        var command = new UpdateProductCommand(product.Id, product.Name, product.Price);
        var dialog = await DialogService.ShowDialogAsync<ProductDialog>(new DialogOptions
        {
            Header = { Title = "Edit Product" },
            Width = "400px",
            Modal = true,
            Parameters = new Dictionary<string, object?>
            {
                ["Content"] = command
            }
        });

        if (!dialog.Cancelled)
        {
            await LoadProductsAsync();
        }
    }

    private async Task OnDeleteProductAsync(ProductResponse product)
    {
        if (IsLoading) return;

        var confirmed = await ConfirmAsync($"Are you sure you want to delete {product.Name}?");
        if (confirmed)
        {
            await SendAsync(
                new DeleteProductCommand(product.Id),
                options: new RequestOptions(SuccessMessage: "Product deleted!"));
            await LoadProductsAsync();
        }
    }
}
