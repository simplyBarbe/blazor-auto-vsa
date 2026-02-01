using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shared.Core;
using Shared.Features.Products.Responses;
using Shared.Features.Products.Create;
using Shared.Features.Products.Get;
using Shared.Core.Validation;
using Client.Extensions;

using Client.Components.SmartComponent;

namespace Client.Features.Products.Components;

public partial class Products : SmartComponentBase
{
    private bool IsBrowser => OperatingSystem.IsBrowser();
    
    // Create product state - use CreateProductCommand directly
    private CreateProductCommand newProduct = new();
    private EditContext editContext = null!;
    private bool isFormValid = false;
    private ProductResponse? createResult;
    private string? createResultMode;
    
    // Search product state
    private int searchId = 1;
    private ProductResponse? searchResult;
    private string? searchResultMode;
    
    // Request history
    private List<HistoryEntry> requestHistory = new();

    protected override void OnInitialized()
    {
        newProduct = new CreateProductCommand { Price = 9.99m };
        editContext = new EditContext(newProduct);
        
        // Track validation state changes without calling Validate() to avoid loops
        editContext.OnValidationStateChanged += (sender, e) =>
        {
            // Check if form is valid by checking if there are any validation messages
            isFormValid = !editContext.GetValidationMessages().Any();
            InvokeAsync(StateHasChanged);
        };
        
        // Initial validation state
        isFormValid = !editContext.GetValidationMessages().Any();
    }

    protected override async Task OnInitializedAsync()
    {
        // Auto-load product with ID 1 on page load
        await SearchProduct();
    }

    private async Task CreateProduct()
    {
        createResult = await SendAsync(newProduct, editContext, "Product created!");
        
        if (createResult != null)
        {
            createResultMode = IsBrowser ? "client" : "server";
            
            requestHistory.Add(new HistoryEntry(
                $"Created: {createResult.Name} (ID: {createResult.Id})",
                createResultMode,
                DateTime.Now
            ));
            
            // Reset form and EditContext
            newProduct = new CreateProductCommand { Price = 9m };
            editContext = new EditContext(newProduct);
            
            // Re-attach validation state tracking
            editContext.OnValidationStateChanged += (sender, e) =>
            {
                isFormValid = !editContext.GetValidationMessages().Any();
                InvokeAsync(StateHasChanged);
            };
            
            isFormValid = !editContext.GetValidationMessages().Any();
        }
    }

    private async Task SearchProduct()
    {
        if (searchId < 1) return;
        
        searchResult = await SendAsync(new GetProductQuery(searchId));
        
        if (searchResult != null)
        {
            searchResultMode = IsBrowser ? "client" : "server";
            
            requestHistory.Add(new HistoryEntry(
                $"Fetched: {searchResult.Name} (ID: {searchResult.Id})",
                searchResultMode,
                DateTime.Now
            ));
        }
    }

    private void ClearHistory()
    {
        requestHistory.Clear();
    }

    private record HistoryEntry(string Action, string Mode, DateTime Timestamp);
}
