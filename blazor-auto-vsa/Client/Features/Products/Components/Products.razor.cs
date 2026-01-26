using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shared.Core;
using Shared.Features.Products.Responses;
using Shared.Features.Products.Create;
using Shared.Features.Products.Get;
using Shared.Core.Validation;
using Client.Extensions;

namespace Client.Features.Products.Components;

public partial class Products
{
    [Inject]
    private IRequestSender RequestSender { get; set; } = default!;

    private bool IsBrowser => OperatingSystem.IsBrowser();
    
    // Create product state - use CreateProductCommand directly
    private CreateProductCommand newProduct = new();
    private EditContext editContext = null!;
    private bool isCreating = false;
    private bool isFormValid = false;
    private ProductResponse? createResult;
    private string? createResultMode;
    private List<ValidationError>? validationErrors;
    
    // Search product state
    private int searchId = 1;
    private bool isSearching = false;
    private ProductResponse? searchResult;
    private string? searchResultMode;
    private string? searchError;
    
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
        isCreating = true;
        createResult = null;
        validationErrors = null;
        
        try
        {
            var mode = IsBrowser ? "client" : "server";
            // Use the command directly - no need to create a new one
            createResult = await RequestSender.Send(newProduct);
            createResultMode = mode;
            
            requestHistory.Add(new HistoryEntry(
                $"Created: {createResult.Name} (ID: {createResult.Id})",
                mode,
                DateTime.Now
            ));
            
            // Reset form and EditContext
            newProduct = new CreateProductCommand { Price = 9m };
            editContext = new EditContext(newProduct);
            
            // Re-attach validation state tracking without calling Validate() to avoid loops
            editContext.OnValidationStateChanged += (sender, e) =>
            {
                // Check if form is valid by checking if there are any validation messages
                isFormValid = !editContext.GetValidationMessages().Any();
                InvokeAsync(StateHasChanged);
            };
            
            // Initial validation state
            isFormValid = !editContext.GetValidationMessages().Any();
        }
        catch (ValidationException ex)
        {
            // Handle server-side validation errors
            validationErrors = ex.Errors;
            
            // Also add errors to EditContext for field-level display
            editContext.AddValidationErrors(ex.Errors);
        }
        catch (Exception ex)
        {
            validationErrors = new List<ValidationError>
            {
                new() { PropertyName = string.Empty, ErrorMessage = $"Errore: {ex.Message}" }
            };
        }
        finally
        {
            isCreating = false;
        }
    }

    private async Task SearchProduct()
    {
        if (searchId < 1) return;
        
        isSearching = true;
        searchResult = null;
        searchError = null;
        
        try
        {
            var mode = IsBrowser ? "client" : "server";
            var query = new GetProductQuery(searchId);
            searchResult = await RequestSender.Send(query);
            searchResultMode = mode;
            
            requestHistory.Add(new HistoryEntry(
                $"Fetched: {searchResult.Name} (ID: {searchResult.Id})",
                mode,
                DateTime.Now
            ));
        }
        catch (ValidationException ex)
        {
            searchError = string.Join(", ", ex.Errors.Select(e => e.ErrorMessage));
        }
        catch (Exception ex)
        {
            searchError = $"Product not found: {ex.Message}";
        }
        finally
        {
            isSearching = false;
        }
    }

    private void ClearHistory()
    {
        requestHistory.Clear();
    }

    private record HistoryEntry(string Action, string Mode, DateTime Timestamp);
}
