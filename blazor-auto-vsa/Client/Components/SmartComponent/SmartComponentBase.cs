using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using Shared.Core;
using Shared.Core.Validation;
using Client.Extensions;

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
    protected bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                Console.WriteLine($"IsLoading changed to: {_isLoading}");
                InvokeAsync(StateHasChanged);
            }
        }
    }
    private bool _isLoading;

    /// <summary>
    /// Executes a request with automatic loading state, error handling, and toast notifications.
    /// </summary>
    protected async Task<TResponse?> SendAsync<TResponse>(
        IRequest<TResponse> request,
        EditContext? editContext = null,
        string? successMessage = null,
        CancellationToken ct = default)
    {
        Console.WriteLine($"SendAsync called for {request.GetType().Name}. IsLoading: {IsLoading}");
        if (IsLoading) return default;
        
        IsLoading = true;
        Console.WriteLine("IsLoading set to true");
        
        try
        {
            if (editContext != null)
            {
                Console.WriteLine("Clearing validation messages in SendAsync");
                editContext.ClearValidationMessages();
                editContext.NotifyValidationStateChanged();
            }

            Console.WriteLine("Sending request via RequestSender");
            var result = await RequestSender.Send(request, ct);
            Console.WriteLine("Request completed successfully");
            
            if (successMessage != null)
            {
                ToastService.ShowSuccess(successMessage);
            }
            
            return result;
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"ValidationException caught in SendAsync with {ex.Errors.Count} errors");
            if (editContext != null)
            {
                Console.WriteLine("Adding validation errors to EditContext");
                editContext.AddValidationErrors(ex.Errors);
            }
            else
            {
                Console.WriteLine("Showing validation errors as toasts");
                foreach (var error in ex.Errors)
                {
                    ToastService.ShowError(error.ErrorMessage);
                }
            }
            return default;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Generic Exception caught in SendAsync: {ex.Message}");
            ToastService.ShowError(ex.Message);
            return default;
        }
        finally
        {
            IsLoading = false;
            Console.WriteLine("IsLoading set to false in finally");
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
