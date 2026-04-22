using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Shared.Core;
using Shared.Features.Products.Responses;
using Shared.Features.Products.Update;
using Shared.Features.Products.Delete;
using Shared.Features.Products.List;
using Shared.Features.Categories.Responses;
using Shared.Features.Categories.List;
using Shared.Features.ProductGroups.Responses;
using Shared.Features.ProductGroups.List;
using Client.Components.Base;

namespace Client.Features.Products.Components;

public partial class ProductsBase : BaseComponent
{
    protected bool IsBrowser => OperatingSystem.IsBrowser();
    protected ListProductQuery Query { get; } = new();
    protected PagedGridController<ProductResponse> GridController { get; private set; } = default!;
    protected PagedDataGrid<ProductResponse>? Grid { get; set; }
    private AsyncState _delete = default!;
    private int? _activeDeleteId;

    [PersistentState]
    public List<ProductResponse>? RestoredItems { get; set; }

    [PersistentState]
    public int RestoredTotalCount { get; set; }

    protected override void OnInitialized()
    {
        GridController = new PagedGridController<ProductResponse>(
            FetchProductsAsync,
            RestoredItems,
            RestoredTotalCount,
            (items, totalCount) =>
            {
                RestoredItems = items.ToList();
                RestoredTotalCount = totalCount;
            });

        Track(GridController.State);
        _delete = UseAsyncState();
        FilterCategoryInit = UseAsyncState();
        FilterGroupListLoad = UseAsyncState<List<ProductGroupResponse>>();
    }

    /// <summary>Initial load: category filter options and default &quot;All groups&quot; row (first async track on the toolbar).</summary>
    protected AsyncState FilterCategoryInit { get; private set; } = default!;

    /// <summary>Loads group filter options when a concrete category is selected (second async track).</summary>
    protected AsyncState<List<ProductGroupResponse>> FilterGroupListLoad { get; private set; } = default!;

    protected List<CategoryResponse> FilterCategories { get; private set; } = [];
    protected List<ProductGroupResponse> FilterGroups { get; private set; } = [];
    protected CategoryResponse? SelectedFilterCategory { get; set; }
    protected ProductGroupResponse? SelectedFilterGroup { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrWhiteSpace(Query.SearchTerm))
        {
            Query.SearchTerm = "";
        }

        await LoadFilterCategoriesAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadFilterCategoriesAsync()
    {
        if (FilterCategoryInit.IsPending)
        {
            return;
        }

        await FilterCategoryInit.RunAsync(async () =>
        {
            var all = new CategoryResponse(0, "All categories");
            var page = await SendAsync(new ListCategoryQuery { PageNumber = 1, PageSize = 500 });
            FilterCategories = new List<CategoryResponse> { all };
            if (page?.Items != null)
            {
                FilterCategories.AddRange(page.Items);
            }

            SelectedFilterCategory ??= all;
            var allGroups = new ProductGroupResponse(0, 0, "", "All groups");
            FilterGroups = [allGroups];
            SelectedFilterGroup ??= allGroups;
        });
    }

    protected async Task OnFilterCategoryChangedAsync(CategoryResponse? value)
    {
        SelectedFilterCategory = value;
        Query.CategoryId = value is { Id: > 0 } ? value.Id : null;
        Query.GroupId = null;
        SelectedFilterGroup = new ProductGroupResponse(0, 0, "", "All groups");
        FilterGroups = [SelectedFilterGroup];

        if (value is { Id: > 0 } c)
        {
            await FilterGroupListLoad.RunAsync(async () =>
            {
                var page = await SendAsync(new ListProductGroupQuery
                {
                    PageNumber = 1,
                    PageSize = 500,
                    CategoryId = c.Id
                });
                var allG = new ProductGroupResponse(0, c.Id, "", "All groups");
                var list = new List<ProductGroupResponse> { allG };
                if (page?.Items != null)
                {
                    list.AddRange(page.Items);
                }

                return list;
            });
            if (FilterGroupListLoad.Data is { } loaded)
            {
                FilterGroups = loaded;
                SelectedFilterGroup = loaded[0];
            }
        }

        await InvokeAsync(StateHasChanged);
    }

    protected Task OnFilterGroupChangedAsync(ProductGroupResponse? value)
    {
        SelectedFilterGroup = value;
        Query.GroupId = value is { Id: > 0 } ? value.Id : null;
        return Task.CompletedTask;
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
        var command = new UpdateProductCommand(
            product.Id,
            product.GroupId,
            product.Name,
            product.Price);
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
            Width = "520px",
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
