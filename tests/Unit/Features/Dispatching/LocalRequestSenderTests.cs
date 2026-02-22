using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Server.Infrastructure.Dispatching;
using Shared.Core;
using Xunit;

namespace Unit.Features.Dispatching;

public class LocalRequestSenderTests
{
    [Fact]
    public async Task Send_should_invoke_handler_and_return_response()
    {
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<TestRequest, TestResponse>, TestHandler>();
        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var sender = new LocalRequestSender(scopeFactory);

        var request = new TestRequest { Value = "Hello" };
        var response = await sender.Send<TestResponse>(request);

        response.Should().NotBeNull();
        response!.Echo.Should().Be("Hello");
    }

    [Fact]
    public async Task Send_should_create_scope_per_request()
    {
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<TestRequest, TestResponse>, TestHandler>();
        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var sender = new LocalRequestSender(scopeFactory);

        var response1 = await sender.Send<TestResponse>(new TestRequest { Value = "A" });
        var response2 = await sender.Send<TestResponse>(new TestRequest { Value = "B" });

        response1!.Echo.Should().Be("A");
        response2!.Echo.Should().Be("B");
    }

    private sealed class TestRequest : IRequest<TestResponse>
    {
        public string Value { get; set; } = "";
    }

    private sealed class TestResponse
    {
        public string Echo { get; set; } = "";
    }

    private sealed class TestHandler : IRequestHandler<TestRequest, TestResponse>
    {
        public Task<TestResponse> Handle(TestRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new TestResponse { Echo = request.Value });
        }
    }
}
