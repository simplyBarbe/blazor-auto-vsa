using System.Net;
using System.Text.Json;
using Client.Dispatching;
using FluentAssertions;
using Moq;
using Shared.Core;
using Shared.Core.Validation;
using Xunit;

namespace Unit.Features.Dispatching;

public class HttpRequestSenderTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static HttpClient CreateClient(HttpMessageHandler handler)
        => new(handler) { BaseAddress = new Uri("http://localhost/") };

    [Fact]
    public async Task Send_Get_with_success_returns_deserialized_response()
    {
        var expected = new TestResponse { Id = 42, Name = "Test" };
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(expected));
        var mapper = new Mock<IRequestEndpointMapper>();
        mapper.Setup(m => m.GetEndpoint(It.IsAny<IRequest<TestResponse>>()))
            .Returns(("/api/test/42", HttpMethod.Get));
        var sender = new HttpRequestSender(CreateClient(handler), mapper.Object);

        var response = await sender.Send<TestResponse>(new TestGetRequest());

        response.Should().NotBeNull();
        response!.Id.Should().Be(42);
        response.Name.Should().Be("Test");
        handler.LastRequest!.Method.Should().Be(HttpMethod.Get);
        handler.LastRequest!.RequestUri!.ToString().Should().Contain("/api/test/42");
    }

    [Fact]
    public async Task Send_Post_with_success_returns_deserialized_response()
    {
        var expected = new TestResponse { Id = 1, Name = "Created" };
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(expected));
        var mapper = new Mock<IRequestEndpointMapper>();
        mapper.Setup(m => m.GetEndpoint(It.IsAny<IRequest<TestResponse>>()))
            .Returns(("/api/test", HttpMethod.Post));
        var sender = new HttpRequestSender(CreateClient(handler), mapper.Object);

        var response = await sender.Send<TestResponse>(new TestCreateRequest { Name = "New" });

        response!.Name.Should().Be("Created");
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
    }

    [Fact]
    public async Task Send_Delete_with_NoContent_returns_default()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.NoContent, "");
        var mapper = new Mock<IRequestEndpointMapper>();
        mapper.Setup(m => m.GetEndpoint(It.IsAny<IRequest<object?>>()))
            .Returns(("/api/test/1", HttpMethod.Delete));
        var sender = new HttpRequestSender(CreateClient(handler), mapper.Object);

        var response = await sender.Send<object?>(new TestDeleteRequest());

        response.Should().BeNull();
        handler.LastRequest!.Method.Should().Be(HttpMethod.Delete);
    }

    [Fact]
    public async Task Send_when_400_with_ValidationErrorResponse_throws_ValidationException()
    {
        var errorResponse = new ValidationErrorResponse
        {
            Errors = [new() { PropertyName = "Name", ErrorMessage = "Name is required" }]
        };
        var handler = new MockHttpMessageHandler(HttpStatusCode.BadRequest, JsonSerializer.Serialize(errorResponse, JsonOptions));
        var mapper = new Mock<IRequestEndpointMapper>();
        mapper.Setup(m => m.GetEndpoint(It.IsAny<IRequest<TestResponse>>()))
            .Returns(("/api/test", HttpMethod.Post));
        var sender = new HttpRequestSender(CreateClient(handler), mapper.Object);

        var act = () => sender.Send<TestResponse>(new TestCreateRequest());

        (await act.Invoking(x => x()).Should().ThrowAsync<ValidationException>())
            .And.Errors.Should().ContainSingle(e => e.PropertyName == "Name" && e.ErrorMessage == "Name is required");
    }

    [Fact]
    public async Task Send_when_404_throws_HttpRequestException()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "Not Found");
        var mapper = new Mock<IRequestEndpointMapper>();
        mapper.Setup(m => m.GetEndpoint(It.IsAny<IRequest<TestResponse>>()))
            .Returns(("/api/test/999", HttpMethod.Get));
        var sender = new HttpRequestSender(CreateClient(handler), mapper.Object);

        var act = () => sender.Send<TestResponse>(new TestGetRequest());

        await act.Invoking(x => x()).Should().ThrowAsync<HttpRequestException>();
    }

    private sealed class TestResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private sealed class TestGetRequest : IRequest<TestResponse> { }

    private sealed class TestCreateRequest : IRequest<TestResponse>
    {
        public string Name { get; set; } = "";
    }

    private sealed class TestDeleteRequest : IRequest<object?> { }

    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        public HttpRequestMessage? LastRequest { get; private set; }

        public MockHttpMessageHandler(HttpStatusCode statusCode, string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content)
            });
        }
    }
}
