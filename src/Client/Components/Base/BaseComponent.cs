using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Shared.Core;
using Shared.Core.Validation;
using Client.Extensions;

namespace Client.Components.Base;

public abstract class BaseComponent : ComponentBase
{
    protected sealed record RequestOptions(bool TrackLoading = true, string? SuccessMessage = null);

    [Inject] protected IRequestSender RequestSender { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ILogger<BaseComponent> Logger { get; set; } = default!;

    protected bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                InvokeAsync(StateHasChanged);
            }
        }
    }
    private bool _isLoading;

    protected async Task<TResponse?> SendAsync<TResponse>(
        IRequest<TResponse> request,
        EditContext? editContext = null,
        RequestOptions? options = null,
        CancellationToken ct = default)
    {
        options ??= new RequestOptions();

        if (options.TrackLoading && IsLoading) return default;

        if (options.TrackLoading)
        {
            IsLoading = true;
        }
        
        try
        {
            if (editContext != null)
            {
                editContext.ClearValidationMessages();
                editContext.NotifyValidationStateChanged();
            }

            var result = await RequestSender.Send(request, ct);
            
            if (options.SuccessMessage != null)
            {
                Logger.LogInformation("{Message}", options.SuccessMessage);
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
                    Logger.LogError("{Message}", error.ErrorMessage);
                }
            }
            return default;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Request failed");
            return default;
        }
        finally
        {
            if (options.TrackLoading)
            {
                IsLoading = false;
            }
        }
    }

    protected async Task<bool> ConfirmAsync(
        string message, 
        string title = "Confirm", 
        string primaryButtonText = "Yes",
        string secondaryButtonText = "No")
    {
        var result = await DialogService.ShowConfirmationAsync(
            message, 
            title, 
            primaryButtonText, 
            secondaryButtonText);

        return !result.Cancelled;
    }

    protected void ShowSuccess(string message) => Logger.LogInformation("{Message}", message);
    protected void ShowError(string message) => Logger.LogError("{Message}", message);
    protected void ShowInfo(string message) => Logger.LogInformation("{Message}", message);
    protected void ShowWarning(string message) => Logger.LogWarning("{Message}", message);
}
