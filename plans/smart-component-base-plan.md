# SmartComponentBase - Clean Architecture Plan

## Overview
A minimal, clean base class for Blazor components that handles common patterns: loading, validation errors, toast notifications, and confirmation dialogs. Uses FluentUI Blazor's `IToastService`, `FluentToastProvider`, and `IDialogService`. Follows KISS principles.

## File Structure

```
blazor-auto-vsa/Client/Components/SmartComponent/
├── SmartComponentBase.cs      # Single base class
└── UiState.cs                 # Simple state enum
```

Plus updates to:
- `MainLayout.razor` - Add `<FluentToastProvider />` and `<FluentDialogProvider />`
- `Program.cs` - Already has `AddFluentUIComponents()` which includes toast and dialog

## Core Design

### 1. SmartComponentBase.cs

```csharp
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using Shared.Core;
using Shared.Core.Validation;

namespace Client.Components.SmartComponent;

/// <summary>
/// Base class for components with loading, validation, toast, and confirmation dialog support.
/// </summary>
public abstract class SmartComponentBase : ComponentBase
{
    [Inject] protected IRequestSender RequestSender { get; set; } = default!;
    [Inject] protected IToastService ToastService { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;

    // Loading
    protected bool IsLoading { get; private set; }

    /// <summary>
    /// Executes a request with automatic loading state, error handling, and toast notifications.
    /// </summary>
    protected async Task<TResponse?> SendAsync<TResponse>(
        IRequest<TResponse> request,
        EditContext? editContext = null,
        string? successMessage = null,
        CancellationToken ct = default)
    {
        IsLoading = true;
        
        try
        {
            var result = await RequestSender.Send(request, ct);
            
            if (successMessage != null)
            {
                ToastService.ShowSuccess(successMessage);
            }
            
            return result;
        }
        catch (ValidationException ex)
        {
            if (editContext != null)
            {
                editContext.AddValidationErrors(ex.Errors);
            }
            else
            {
                foreach (var error in ex.Errors)
                {
                    ToastService.ShowError(error.ErrorMessage);
                }
            }
            return default;
        }
        catch (Exception ex)
        {
            ToastService.ShowError(ex.Message);
            return default;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Shows a confirmation dialog and returns true if confirmed.
    /// </summary>
    protected async Task<bool> ConfirmAsync(
        string message, 
        string title = "Confirm", 
        string primaryButtonText = "Yes",
        string secondaryButtonText = "No")
    {
        var dialog = await DialogService.ShowConfirmationAsync(
            message, 
            title, 
            primaryButtonText, 
            secondaryButtonText);
        
        var result = await dialog.Result;
        return !result.Cancelled;
    }

    protected void ShowSuccess(string message) => ToastService.ShowSuccess(message);
    protected void ShowError(string message) => ToastService.ShowError(message);
    protected void ShowInfo(string message) => ToastService.ShowInfo(message);
    protected void ShowWarning(string message) => ToastService.ShowWarning(message);
}
```

### 2. MainLayout.razor Update

Add `<FluentToastProvider />` and `<FluentDialogProvider />` to enable toasts and dialogs globally:

```razor
@inherits LayoutComponentBase

<FluentLayout>
    <FluentHeader>Server</FluentHeader>
    <FluentStack Class="main" Orientation="Orientation.Horizontal" Width="100%">
        <NavMenu />
        <FluentBodyContent Class="body-content">
            <div class="content">
                @Body
            </div>
        </FluentBodyContent>
    </FluentStack>
    <FluentFooter>
        <a href="https://www.fluentui-blazor.net" target="_blank">Documentation</a>
        <FluentSpacer />
        <a href="https://learn.microsoft.com/aspnet/core/blazor" target="_blank">About Blazor</a>
    </FluentFooter>
</FluentLayout>

<FluentToastProvider />
<FluentDialogProvider />

<div id="blazor-error-ui" data-nosnippet>
    An unhandled error has occurred.
    <a href="." class="reload">Reload</a>
    <span class="dismiss">🗙</span>
</div>
```

## Usage Example

### Create Operation
```csharp
public partial class Products : SmartComponentBase
{
    private CreateProductCommand newProduct = new();
    private EditContext editContext = null!;

    protected override void OnInitialized()
    {
        editContext = new EditContext(newProduct);
    }

    private async Task CreateProduct()
    {
        var result = await SendAsync(
            newProduct, 
            editContext, 
            successMessage: "Product created!"
        );
        
        if (result != null)
        {
            newProduct = new();
            editContext = new EditContext(newProduct);
        }
    }
}
```

### Delete with Confirmation
```csharp
private async Task DeleteProduct(int id)
{
    if (!await ConfirmAsync(
        "Are you sure you want to delete this product?", 
        title: "Delete Product"))
    {
        return;
    }

    await SendAsync(
        new DeleteProductCommand(id),
        successMessage: "Product deleted!"
    );
}
```

### Cancellation Token Usage
```csharp
private CancellationTokenSource? _cts;

private async Task LoadProducts()
{
    _cts?.Cancel();
    _cts = new CancellationTokenSource();
    
    var result = await SendAsync(
        new ListProductQuery(),
        ct: _cts.Token
    );
    
    if (result != null)
    {
        Products = result.Items;
    }
}

public void Dispose()
{
    _cts?.Cancel();
    _cts?.Dispose();
}
```

### Razor Template

```razor
<EditForm EditContext="editContext" OnValidSubmit="CreateProduct">
    <FluentValidationValidator />
    
    <FluentTextField @bind-Value="newProduct.Name" Label="Name" />
    <FluentValidationMessage For="@(() => newProduct.Name)" />
    
    <FluentButton Type="ButtonType.Submit" 
                  Loading="IsLoading" 
                  Disabled="IsLoading">
        Create
    </FluentButton>
</EditForm>

<FluentButton OnClick="@(() => DeleteProduct(product.Id))" 
              Appearance="Appearance.Stealth"
              Disabled="IsLoading">
    <FluentIcon Value="@new Icons.Regular.Size16.Delete()" />
</FluentButton>
```

## Key Principles Applied

1. **Single Responsibility**: Base class handles request execution pattern
2. **Minimal API**: `SendAsync` and `ConfirmAsync` cover most needs
3. **FluentUI Native**: Uses `IToastService`, `IDialogService`, `FluentButton`
4. **No Abstractions**: Direct use of existing services
5. **Global Toast/Dialog**: Providers in MainLayout, no per-component markup
6. **Clean Errors**: Validation errors go to EditContext or Toast
7. **Cancellation Ready**: Accepts CancellationToken for async operations

## Integration

Works with existing:
- `IRequestSender` / `HttpRequestSender`
- `FluentValidationValidator`
- `EditContextExtensions`
- `ValidationException`
- `AddFluentUIComponents()` (already in Program.cs)

Changes needed:
- Add `<FluentToastProvider />` and `<FluentDialogProvider />` to MainLayout.razor
- Inherit from `SmartComponentBase` instead of `ComponentBase`

## Multiple Concurrent Operations (Explanation)

The current design uses a single `IsLoading` boolean. For multiple concurrent operations (e.g., loading products AND categories simultaneously), there are two approaches:

### Option A: Operation Keys (Not Implemented - Keep It Simple)
```csharp
// Track multiple operations by key
protected Dictionary<string, int> _activeOperations = new();

protected bool IsLoading => _activeOperations.Count > 0;
protected bool IsOperationActive(string key) => _activeOperations.ContainsKey(key);

protected void StartOperation(string key) { /* increment counter */ }
protected void EndOperation(string key) { /* decrement counter, remove if 0 */ }

// Usage:
await StartOperation("products");
var products = await SendAsync(new ListProductQuery());
EndOperation("products");

// In razor:
<FluentButton Loading="IsOperationActive("products")">Load Products</FluentButton>
<FluentButton Loading="IsOperationActive("categories")">Load Categories</FluentButton>
```

### Option B: Component-Level Properties (Recommended for KISS)
```csharp
public partial class Dashboard : SmartComponentBase
{
    private bool IsLoadingProducts { get; set; }
    private bool IsLoadingCategories { get; set; }
    
    private async Task LoadProducts()
    {
        IsLoadingProducts = true;
        // ... call API
        IsLoadingProducts = false;
    }
}
```

For now, we keep the simple `IsLoading` property. Components can add their own boolean flags if they need granular loading states.