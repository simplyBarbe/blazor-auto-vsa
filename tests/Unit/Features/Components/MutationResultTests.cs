using Client.Components.Base;
using FluentAssertions;

namespace Unit.Features.Components;

public class MutationResultTests
{
    [Fact]
    public async Task ExecuteAsync_should_transition_from_pending_to_success()
    {
        var result = new MutationResult();
        var completion = new TaskCompletionSource<bool>();
        var sawPendingInsideExecution = false;

        var execution = result.ExecuteAsync(async () =>
        {
            sawPendingInsideExecution = result.IsPending;
            await completion.Task;
        });

        result.IsPending.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsError.Should().BeFalse();

        completion.SetResult(true);
        var success = await execution;

        sawPendingInsideExecution.Should().BeTrue();
        success.Should().BeTrue();
        result.IsPending.Should().BeFalse();
        result.IsSuccess.Should().BeTrue();
        result.IsError.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_should_transition_from_pending_to_error()
    {
        var result = new MutationResult();
        var exception = new InvalidOperationException("boom");

        var success = await result.ExecuteAsync(() => Task.FromException(exception));

        success.Should().BeFalse();
        result.IsPending.Should().BeFalse();
        result.IsSuccess.Should().BeFalse();
        result.IsError.Should().BeTrue();
        result.Error.Should().BeSameAs(exception);
    }

    [Fact]
    public async Task ExecuteAsync_should_update_state_across_multiple_sequential_executions()
    {
        var result = new MutationResult();

        var first = await result.ExecuteAsync(() => Task.CompletedTask);
        var second = await result.ExecuteAsync(() => Task.CompletedTask);

        first.Should().BeTrue();
        second.Should().BeTrue();
        result.IsPending.Should().BeFalse();
        result.IsSuccess.Should().BeTrue();
        result.IsError.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_should_apply_last_started_wins_during_concurrent_executions()
    {
        var result = new MutationResult();
        var firstCompletion = new TaskCompletionSource<bool>();
        var secondCompletion = new TaskCompletionSource<bool>();

        var firstExecution = result.ExecuteAsync(() => firstCompletion.Task);
        var secondExecution = result.ExecuteAsync(() => secondCompletion.Task);

        result.IsPending.Should().BeTrue();

        secondCompletion.SetResult(true);
        var secondSuccess = await secondExecution;

        secondSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.IsError.Should().BeFalse();
        result.IsPending.Should().BeTrue();

        firstCompletion.SetResult(true);
        var firstSuccess = await firstExecution;

        firstSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.IsError.Should().BeFalse();
        result.IsPending.Should().BeFalse();
        result.Error.Should().BeNull();
    }
}
