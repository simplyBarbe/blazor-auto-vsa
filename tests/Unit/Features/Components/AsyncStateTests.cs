using Client.Components.Base;
using FluentAssertions;

namespace Unit.Features.Components;

public class AsyncStateTests
{
    [Fact]
    public async Task Generic_RunAsync_should_transition_from_pending_to_success_with_data()
    {
        var state = new AsyncState<string>();
        var completion = new TaskCompletionSource<string?>();
        var sawPendingInside = false;

        var execution = state.RunAsync(async () =>
        {
            sawPendingInside = state.IsPending;
            return await completion.Task;
        });

        state.IsPending.Should().BeTrue();
        state.IsSuccess.Should().BeFalse();
        state.IsError.Should().BeFalse();

        completion.SetResult("done");
        var value = await execution;

        sawPendingInside.Should().BeTrue();
        value.Should().Be("done");
        state.Data.Should().Be("done");
        state.IsPending.Should().BeFalse();
        state.IsSuccess.Should().BeTrue();
        state.IsError.Should().BeFalse();
        state.Error.Should().BeNull();
    }

    [Fact]
    public async Task Generic_RunAsync_should_transition_from_pending_to_error()
    {
        var state = new AsyncState<string>();
        var exception = new InvalidOperationException("boom");

        var value = await state.RunAsync(() => Task.FromException<string?>(exception));

        value.Should().BeNull();
        state.IsPending.Should().BeFalse();
        state.IsSuccess.Should().BeFalse();
        state.IsError.Should().BeTrue();
        state.Error.Should().BeSameAs(exception);
    }

    [Fact]
    public async Task NonGeneric_RunAsync_should_transition_from_pending_to_success()
    {
        var state = new AsyncState();
        var success = await state.RunAsync(() => Task.CompletedTask);

        success.Should().BeTrue();
        state.IsPending.Should().BeFalse();
        state.IsSuccess.Should().BeTrue();
        state.IsError.Should().BeFalse();
        state.Error.Should().BeNull();
    }

    [Fact]
    public async Task NonGeneric_RunAsync_should_transition_from_pending_to_error()
    {
        var state = new AsyncState();
        var exception = new InvalidOperationException("boom");

        var success = await state.RunAsync(() => Task.FromException(exception));

        success.Should().BeFalse();
        state.IsPending.Should().BeFalse();
        state.IsSuccess.Should().BeFalse();
        state.IsError.Should().BeTrue();
        state.Error.Should().BeSameAs(exception);
    }

    [Fact]
    public async Task OnChange_should_fire_on_start_and_completion()
    {
        var state = new AsyncState<int>();
        var changes = 0;
        state.OnChange += () => changes++;

        await state.RunAsync(() => Task.FromResult(42));

        changes.Should().Be(2);
    }
}
