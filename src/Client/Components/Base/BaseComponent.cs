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

    protected QueryResult<T> CreateQuery<T>()
    {
        var query = new QueryResult<T>();
        WireStateChange(handler => query.OnChange += handler, handler => query.OnChange -= handler);
        return query;
    }

    protected MutationResult CreateMutation()
    {
        var mutation = new MutationResult();
        WireStateChange(handler => mutation.OnChange += handler, handler => mutation.OnChange -= handler);
        return mutation;
    }

    protected PagedGridController<TItem> CreatePagedGridController<TItem>(
        Func<int, int, Task<PagedResult<TItem>?>> fetchPageAsync,
        int itemsPerPage,
        IReadOnlyList<TItem>? restoredItems = null,
        int restoredTotalCount = 0,
        Action<IReadOnlyList<TItem>, int>? snapshotChanged = null)
    {
        return new PagedGridController<TItem>(
            CreateQuery<PagedResult<TItem>>(),
            fetchPageAsync,
            itemsPerPage,
            restoredItems,
            restoredTotalCount,
            snapshotChanged);
    }

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

    private void WireStateChange(Action<Action> subscribe, Action<Action> unsubscribe)
    {
        Action handler = () => _ = InvokeAsync(StateHasChanged);
        subscribe(handler);
        _disposeActions.Add(() => unsubscribe(handler));
    }
}
