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

public partial class Products : SmartListBase<ProductResponse, ListProductQuery>
{
    private readonly PaginationState _pagination = new() { ItemsPerPage = 3 };
    private FluentDataGrid<ProductResponse>? _grid;

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

    protected async Task<TResponse?> SendWithoutLoadingAsync<TResponse>(IRequest<TResponse> request)
    {
        try
        {
            return await RequestSender.Send(request);
        }
        catch (Exception ex)
        {
            ToastService.ShowError(ex.Message);
            return default;
        }
    }

    private async ValueTask<GridItemsProviderResult<ProductResponse>> ProductProvider(GridItemsProviderRequest<ProductResponse> request)
    {
        Query.PageNumber = (request.StartIndex / (request.Count ?? _pagination.ItemsPerPage)) + 1;
        Query.PageSize = request.Count ?? _pagination.ItemsPerPage;

        var result = await SendWithoutLoadingAsync(Query);

        if (result == null)
        {
            return GridItemsProviderResult.From(new List<ProductResponse>(), TotalCount);
        }

        Items = result.Items.AsQueryable();
        return GridItemsProviderResult.From(result.Items.ToList(), result.TotalCount);
    }

    private async Task LoadProductsAsync() => await LoadDataAsync();

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
