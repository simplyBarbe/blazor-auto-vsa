namespace Client.Components.Base;

/// <summary>
/// Async state for an operation that produces a value (query-like).
/// Tracks IsPending / Data / Error in a Blazor-friendly way.
/// </summary>
public sealed class AsyncState<T>
{
    public T? Data { get; private set; }
    public bool IsPending { get; private set; }
    public Exception? Error { get; private set; }
    public bool IsSuccess { get; private set; }
    public bool IsError => Error is not null;

    public event Action? OnChange;

    public async Task<T?> RunAsync(Func<Task<T?>> fn)
    {
        ArgumentNullException.ThrowIfNull(fn);

        IsPending = true;
        IsSuccess = false;
        Error = null;
        OnChange?.Invoke();

        try
        {
            Data = await fn();
            IsSuccess = true;
            return Data;
        }
        catch (Exception ex)
        {
            Error = ex;
            return default;
        }
        finally
        {
            IsPending = false;
            OnChange?.Invoke();
        }
    }
}

/// <summary>
/// Async state for an operation that does not produce a value (mutation-like).
/// </summary>
public sealed class AsyncState
{
    public bool IsPending { get; private set; }
    public Exception? Error { get; private set; }
    public bool IsSuccess { get; private set; }
    public bool IsError => Error is not null;

    public event Action? OnChange;

    public async Task<bool> RunAsync(Func<Task> fn)
    {
        ArgumentNullException.ThrowIfNull(fn);

        IsPending = true;
        IsSuccess = false;
        Error = null;
        OnChange?.Invoke();

        try
        {
            await fn();
            IsSuccess = true;
            return true;
        }
        catch (Exception ex)
        {
            Error = ex;
            return false;
        }
        finally
        {
            IsPending = false;
            OnChange?.Invoke();
        }
    }
}
