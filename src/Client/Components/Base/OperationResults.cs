namespace Client.Components.Base;

public sealed class QueryResult<T>
{
    private readonly object _gate = new();
    private long _executionId;
    private long _latestExecutionId;
    private int _activeExecutions;

    public T? Data { get; private set; }
    public bool IsPending { get; private set; }
    public bool IsError { get; private set; }
    public Exception? Error { get; private set; }
    public bool IsSuccess { get; private set; }

    public event Action? OnChange;

    public async Task<T?> ExecuteAsync(Func<Task<T?>> fn)
    {
        ArgumentNullException.ThrowIfNull(fn);

        var executionId = BeginExecution();
        NotifyChange();

        try
        {
            var data = await fn();
            CompleteSuccess(executionId, data);
            return data;
        }
        catch (Exception ex)
        {
            CompleteError(executionId, ex);
            return default;
        }
    }

    private long BeginExecution()
    {
        lock (_gate)
        {
            var executionId = ++_executionId;
            _latestExecutionId = executionId;
            _activeExecutions++;

            IsPending = true;
            IsError = false;
            Error = null;
            IsSuccess = false;

            return executionId;
        }
    }

    private void CompleteSuccess(long executionId, T? data)
    {
        lock (_gate)
        {
            _activeExecutions--;

            if (executionId == _latestExecutionId)
            {
                Data = data;
                IsError = false;
                Error = null;
                IsSuccess = true;
            }

            IsPending = _activeExecutions > 0;
        }

        NotifyChange();
    }

    private void CompleteError(long executionId, Exception error)
    {
        lock (_gate)
        {
            _activeExecutions--;

            if (executionId == _latestExecutionId)
            {
                IsError = true;
                Error = error;
                IsSuccess = false;
            }

            IsPending = _activeExecutions > 0;
        }

        NotifyChange();
    }

    private void NotifyChange() => OnChange?.Invoke();
}

public sealed class MutationResult
{
    private readonly object _gate = new();
    private long _executionId;
    private long _latestExecutionId;
    private int _activeExecutions;

    public bool IsPending { get; private set; }
    public bool IsError { get; private set; }
    public Exception? Error { get; private set; }
    public bool IsSuccess { get; private set; }

    public event Action? OnChange;

    public async Task<bool> ExecuteAsync(Func<Task> fn)
    {
        ArgumentNullException.ThrowIfNull(fn);

        var executionId = BeginExecution();
        NotifyChange();

        try
        {
            await fn();
            CompleteSuccess(executionId);
            return true;
        }
        catch (Exception ex)
        {
            CompleteError(executionId, ex);
            return false;
        }
    }

    private long BeginExecution()
    {
        lock (_gate)
        {
            var executionId = ++_executionId;
            _latestExecutionId = executionId;
            _activeExecutions++;

            IsPending = true;
            IsError = false;
            Error = null;
            IsSuccess = false;

            return executionId;
        }
    }

    private void CompleteSuccess(long executionId)
    {
        lock (_gate)
        {
            _activeExecutions--;

            if (executionId == _latestExecutionId)
            {
                IsError = false;
                Error = null;
                IsSuccess = true;
            }

            IsPending = _activeExecutions > 0;
        }

        NotifyChange();
    }

    private void CompleteError(long executionId, Exception error)
    {
        lock (_gate)
        {
            _activeExecutions--;

            if (executionId == _latestExecutionId)
            {
                IsError = true;
                Error = error;
                IsSuccess = false;
            }

            IsPending = _activeExecutions > 0;
        }

        NotifyChange();
    }

    private void NotifyChange() => OnChange?.Invoke();
}
