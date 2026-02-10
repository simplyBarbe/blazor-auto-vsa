using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Pipeline;
using Shared.Core.Validation;
using Server.Infrastructure.Pipeline;
using Server.Extensions;
using FluentAssertions;
using Xunit;

namespace Server.Tests.Integration;

public class PipelineRegistrationTests
{
    [Fact]
    public void AddRequestPipeline_ShouldRegisterValidatorAndBehavior()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRequestPipeline();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        // Check IAsyncRequestValidator registration
        var validator = serviceProvider.GetService<IAsyncRequestValidator>();
        validator.Should().NotBeNull();
        validator.Should().BeOfType<FluentValidationRequestValidator>();

        // Check IPipelineBehavior registration
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TestRequest, TestResponse>>().ToList();
        behaviors.Should().Contain(b => b is ValidationBehavior<TestRequest, TestResponse>);
    }

    public class TestRequest : Shared.Core.IRequest<TestResponse> { }
    public class TestResponse { }
}
