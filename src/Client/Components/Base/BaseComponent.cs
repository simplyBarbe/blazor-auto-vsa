using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using Shared.Core;
using Shared.Core.Validation;
using Client.Extensions;

namespace Client.Components.Base;

public abstract class BaseComponent : ComponentBase, IDisposable
{
    protected sealed record RequestOptions(string? SuccessMessage = null);

    [Inject] protected IRequestSender RequestSender { get; set; } = default!;
    [Inject] protected IToastService ToastService { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;

    private readonly List<Action> _disposeActions = [];

    protected async Task<TResponse?> SendAsync<TResponse>(
        IRequest<TResponse> request,
        EditContext? editContext = null,
        RequestOptions? options = null,
        CancellationToken ct = default)
    {
        options ??= new RequestOptions();

        try
        {
            if (editContext != null)
            {
                editContext.ClearValidationMessages();
                editContext.NotifyValidationStateChanged();
            }

            var result = await RequestSender.Send(request, ct);

            if (!string.IsNullOrWhiteSpace(options.SuccessMessage))
            {
                ToastService.ShowSuccess(options.SuccessMessage);
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

            throw;
        }
        catch (Exception ex)
        {
            ToastService.ShowError(ex.Message);
            throw;
        }
    }

    /// <summary>Subscribes an AsyncState's OnChange to StateHasChanged and auto-unsubscribes on dispose.</summary>
    protected TState Track<TState>(TState state, Action<Action> subscribe, Action<Action> unsubscribe)
    {
        Action handler = () => _ = InvokeAsync(StateHasChanged);
        subscribe(handler);
        _disposeActions.Add(() => unsubscribe(handler));
        return state;
    }

    protected AsyncState<T> Track<T>(AsyncState<T> state)
        => Track(state, h => state.OnChange += h, h => state.OnChange -= h);

    protected AsyncState Track(AsyncState state)
        => Track(state, h => state.OnChange += h, h => state.OnChange -= h);

    /// <summary>
    /// Creates and tracks a component-owned AsyncState instance.
    /// Use Track(...) directly for states created outside this component (for example GridController.State).
    /// </summary>
    protected AsyncState UseAsyncState() => Track(new AsyncState());

    /// <summary>
    /// Creates and tracks a component-owned AsyncState instance that stores data.
    /// Use Track(...) directly for states created outside this component (for example GridController.State).
    /// </summary>
    protected AsyncState<T> UseAsyncState<T>() => Track(new AsyncState<T>());

    protected async Task<bool> ConfirmAsync(
        string message,
        string title = "Confirm",
        string primaryButtonText = "Yes",
        string secondaryButtonText = "No")
    {
        var dialog = await DialogService.ShowConfirmationAsync(
            message,
            primaryButtonText,
            secondaryButtonText,
            title);

        var result = await dialog.Result;
        return !result.Cancelled;
    }

    protected void ShowSuccess(string message) => ToastService.ShowSuccess(message);
    protected void ShowError(string message) => ToastService.ShowError(message);
    protected void ShowInfo(string message) => ToastService.ShowInfo(message);
    protected void ShowWarning(string message) => ToastService.ShowWarning(message);

    public virtual void Dispose()
    {
        foreach (var disposeAction in _disposeActions)
        {
            disposeAction();
        }

        _disposeActions.Clear();
    }
}
