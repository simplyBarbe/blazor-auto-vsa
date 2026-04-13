using Client.Components.Base;
using FluentAssertions;

namespace Unit.Features.Components;

public class QueryResultTests
{
    [Fact]
    public async Task ExecuteAsync_should_transition_from_pending_to_success()
    {
        var result = new QueryResult<string>();
        var completion = new TaskCompletionSource<string>();
        var sawPendingInsideExecution = false;

        var execution = result.ExecuteAsync(async () =>
        {
            sawPendingInsideExecution = result.IsPending;
            return await completion.Task;
        });

        result.IsPending.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsError.Should().BeFalse();

        completion.SetResult("done");
        var value = await execution;

        sawPendingInsideExecution.Should().BeTrue();
        value.Should().Be("done");
        result.Data.Should().Be("done");
        result.IsPending.Should().BeFalse();
        result.IsSuccess.Should().BeTrue();
        result.IsError.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_should_transition_from_pending_to_error()
    {
        var result = new QueryResult<string>();
        var exception = new InvalidOperationException("boom");

        var value = await result.ExecuteAsync(() => Task.FromException<string>(exception));

        value.Should().BeNull();
        result.Data.Should().BeNull();
        result.IsPending.Should().BeFalse();
        result.IsSuccess.Should().BeFalse();
        result.IsError.Should().BeTrue();
        result.Error.Should().BeSameAs(exception);
    }

    [Fact]
    public async Task ExecuteAsync_should_update_state_across_multiple_sequential_executions()
    {
        var result = new QueryResult<string>();

        var first = await result.ExecuteAsync(() => Task.FromResult("first"));
        var second = await result.ExecuteAsync(() => Task.FromResult("second"));

        first.Should().Be("first");
        second.Should().Be("second");
        result.Data.Should().Be("second");
        result.IsPending.Should().BeFalse();
        result.IsSuccess.Should().BeTrue();
        result.IsError.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_should_apply_last_started_wins_during_concurrent_executions()
    {
        var result = new QueryResult<string>();
        var firstCompletion = new TaskCompletionSource<string>();
        var secondCompletion = new TaskCompletionSource<string>();

        var firstExecution = result.ExecuteAsync(() => firstCompletion.Task);
        var secondExecution = result.ExecuteAsync(() => secondCompletion.Task);

        result.IsPending.Should().BeTrue();

        secondCompletion.SetResult("second");
        var secondValue = await secondExecution;

        secondValue.Should().Be("second");
        result.Data.Should().Be("second");
        result.IsSuccess.Should().BeTrue();
        result.IsError.Should().BeFalse();
        result.IsPending.Should().BeTrue();

        firstCompletion.SetResult("first");
        var firstValue = await firstExecution;

        firstValue.Should().Be("first");
        result.Data.Should().Be("second");
        result.IsSuccess.Should().BeTrue();
        result.IsError.Should().BeFalse();
        result.IsPending.Should().BeFalse();
        result.Error.Should().BeNull();
    }
}
