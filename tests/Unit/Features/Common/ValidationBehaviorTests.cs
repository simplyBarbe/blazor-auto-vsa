using FluentAssertions;
using Moq;
using Server.Infrastructure.Pipeline;
using Shared.Core;
using Shared.Core.Pipeline;
using Shared.Core.Validation;
using Xunit;

namespace Unit.Features.Common;

public class ValidationBehaviorTests
{
    private sealed class TestRequest : IRequest<TestResponse>
    {
    }

    private sealed class TestResponse
    {
        public string Value { get; set; } = "";
    }

    [Fact]
    public async Task Handle_when_valid_should_call_next_and_return_result()
    {
        var validator = new Mock<IAsyncRequestValidator>();
        validator.Setup(x => x.ValidateAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationResult.Success());

        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validator.Object);
        var request = new TestRequest();
        var expected = new TestResponse { Value = "ok" };
        RequestHandlerDelegate<TestResponse> next = () => Task.FromResult(expected);

        var result = await behavior.Handle(request, next, CancellationToken.None);

        result.Should().BeSameAs(expected);
        validator.Verify(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_when_invalid_should_throw_ValidationException_and_not_call_next()
    {
        var errors = new List<ValidationError> { new() { PropertyName = "Name", ErrorMessage = "Required" } };
        var validator = new Mock<IAsyncRequestValidator>();
        validator.Setup(x => x.ValidateAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationResult.Failure(errors));

        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validator.Object);
        var request = new TestRequest();
        var nextCalled = false;
        RequestHandlerDelegate<TestResponse> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(new TestResponse());
        };

        var act = () => behavior.Handle(request, next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Count == 1 && ex.Errors[0].ErrorMessage == "Required");
        nextCalled.Should().BeFalse();
    }
}
