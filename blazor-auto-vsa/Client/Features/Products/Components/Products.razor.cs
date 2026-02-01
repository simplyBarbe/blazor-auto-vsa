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

using Client.Components.SmartComponent;

namespace Client.Features.Products.Components;

public partial class Products : SmartComponentBase
{
    private bool IsBrowser => OperatingSystem.IsBrowser();
    
    private IQueryable<ProductResponse>? products;

    protected override async Task OnInitializedAsync()
    {
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        var result = await SendAsync(new ListProductQuery { PageSize = 100 });
        if (result != null)
        {
            products = result.Items.AsQueryable();
        }
    }

    private async Task OnAddProductAsync()
    {
        if (IsLoading) return;

        var command = new UpdateProductCommand();
        var dialog = await DialogService.ShowDialogAsync<ProductDialog>(command, new DialogParameters
        {
            Title = "Add Product",
            Width = "400px",
            TrapFocus = true,
            Modal = true,
            PreventScroll = true
        });

        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            await LoadProductsAsync();
        }
    }

    private async Task OnEditProductAsync(ProductResponse product)
    {
        if (IsLoading) return;

        var command = new UpdateProductCommand(product.Id, product.Name, product.Price);
        var dialog = await DialogService.ShowDialogAsync<ProductDialog>(command, new DialogParameters
        {
            Title = "Edit Product",
            Width = "400px",
            TrapFocus = true,
            Modal = true,
            PreventScroll = true
        });

        var result = await dialog.Result;
        if (!result.Cancelled)
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
            await SendAsync(new DeleteProductCommand(product.Id), successMessage: "Product deleted!");
            await LoadProductsAsync();
        }
    }
}
